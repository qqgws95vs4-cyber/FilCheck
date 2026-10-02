using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FilTjek {
public class Options { public string Root, Extensions; public int Years, Mode; public bool Recursive; public DateTime Now; }
public class Result {
 public long Seen, Matches, Bytes, Errors, Skipped; public bool Cancelled; public string Directory, Fatal;
 public List<string[]> Preview = new List<string[]>();
}
public static class Scanner {
 public static string Cell(string s) { s=s??""; if(s.Length>0 && "=+-@\t\r\n".IndexOf(s[0])>=0) s="'"+s; return "\""+s.Replace("\"","\"\"")+"\""; }
 public static void Row(StreamWriter w, params string[] cells) { w.WriteLine(String.Join(";",cells.Select(Cell))); }
 public static string Stamp(DateTime d) { return d.ToString("yyyy-MM-dd HH:mm:ss zzz"); }
 public static bool Candidate(DateTime modified, DateTime accessed, DateTime cutoff, int mode) { return mode==0 ? modified<cutoff : mode==1 ? accessed<cutoff : modified<cutoff && accessed<cutoff; }
 public static Result Run(Options o, Func<bool> cancel, Action<Result> progress) {
  Result r=new Result(); r.Directory=Path.Combine(Path.GetTempPath(),"FilTjek-"+Guid.NewGuid().ToString("N")); System.IO.Directory.CreateDirectory(r.Directory);
  DateTime cutoff=o.Now.AddYears(-o.Years);
  var ext=new HashSet<string>((o.Extensions??"").Split(new char[]{';',',',' '},StringSplitOptions.RemoveEmptyEntries).Select(s=>"."+s.Trim().TrimStart('*','.')),StringComparer.OrdinalIgnoreCase);
  using(var report=new StreamWriter(Path.Combine(r.Directory,"Filer.csv"),false,new UTF8Encoding(true)))
  using(var errors=new StreamWriter(Path.Combine(r.Directory,"Problemer.csv"),false,new UTF8Encoding(true))) {
   Row(report,"Filsti","Filtype","Bytes","Sidst ændret","Sidste registrerede adgang","Oprettet","Filter","Skæringsdato","Scanningstidspunkt");
   Row(errors,"Sti","Type","Beskrivelse");
   var pending=new Stack<string>(); pending.Push(o.Root); var tick=System.Diagnostics.Stopwatch.StartNew();
   while(pending.Count>0 && !cancel()) {
    string dir=pending.Pop();
    try {
     foreach(var info in new DirectoryInfo(dir).EnumerateFileSystemInfos()) {
      if(cancel()) break;
      try {
       var attrs=info.Attributes;
       if((attrs&FileAttributes.ReparsePoint)!=0) { r.Skipped++; Row(errors,info.FullName,"Sprunget over","Link eller reparse point; følges ikke"); continue; }
       if((attrs&FileAttributes.Directory)!=0) { if(o.Recursive) pending.Push(info.FullName); continue; }
       r.Seen++; var f=(FileInfo)info;
       if(ext.Count>0 && !ext.Contains(f.Extension)) continue;
       DateTime m=f.LastWriteTime, a=f.LastAccessTime, c=f.CreationTime;
       if(m.Year<1900 || a.Year<1900) { r.Errors++; Row(errors,f.FullName,"Ukendt dato","Tidsstempel mangler eller er ugyldigt"); continue; }
       if(!Candidate(m,a,cutoff,o.Mode)) continue;
       long size=f.Length; r.Matches++; r.Bytes+=size;
       string[] row={f.FullName,f.Extension,size.ToString(System.Globalization.CultureInfo.InvariantCulture),Stamp(m),Stamp(a),Stamp(c),Modes[o.Mode],Stamp(cutoff),Stamp(o.Now)};
       Row(report,row); if(r.Preview.Count<500) r.Preview.Add(row);
      } catch(Exception ex) { if(!(ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)) throw; r.Errors++; Row(errors,info.FullName,"Læsning mislykkedes",ex.Message); }
      finally { if(tick.ElapsedMilliseconds>300) { progress(r); tick.Restart(); } }
     }
    } catch(Exception ex) { if(!(ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)) throw; r.Errors++; Row(errors,dir,"Mappe kunne ikke læses fuldt",ex.Message); }
    progress(r);
   }
   r.Cancelled=cancel();
  }
  File.WriteAllText(Path.Combine(r.Directory,"Rapport.txt"),"FilTjek – rapport\r\n"+
   "Status: "+(r.Cancelled?"AFBRUDT – delvis rapport":r.Errors>0 || r.Skipped>0?"Afsluttet med fejl eller udeladelser":"Afsluttet")+"\r\nSti: "+o.Root+
   "\r\nScanning: "+Stamp(o.Now)+"\r\nFilter: "+Modes[o.Mode]+"\r\nÆldre end: "+o.Years+" år; før "+Stamp(cutoff)+
   "\r\nUndermapper: "+o.Recursive+"\r\nFiltyper: "+(ext.Count==0?"Alle":o.Extensions)+"\r\nUndersøgte filer: "+r.Seen+"\r\nKandidater: "+r.Matches+
   "\r\nSamlet logisk størrelse (bytes): "+r.Bytes+"\r\nFejl: "+r.Errors+"\r\nLinks/reparse points sprunget over: "+r.Skipped+
   "\r\n\r\nFiler.csv indeholder alle kandidater. Problemer.csv indeholder fejl og udeladelser.\r\n"+
   "Dette er kandidater til ubrugte filer, ikke bevis for manglende brug. Sidste adgang kan være deaktiveret, forsinket eller påvirket af backup og antivirus.\r\n"+
   "Programmet læser metadata, ikke filindhold. Det kan ikke rekonstruere åbningshistorik eller vise, hvem der åbnede en fil.\r\n"+
   "Links og andre reparse points springes over. Netværksdelingens rettigheder og tidsregistrering gælder. Størrelse er logisk størrelse, ikke garanteret frigørelig diskplads.\r\n"+
   "Datoer vises i denne computers lokale tidszone. Nye kopier kan bevare gamle ændringsdatoer; kontrollér også Oprettet.\r\n",new UTF8Encoding(true));
  return r;
 }
 public static readonly string[] Modes={"Sidst ændret","Sidste registrerede adgang","Både ændret og registreret adgang"};
}
public class MainForm : Form {
 TextBox path=new TextBox(), types=new TextBox(); ComboBox years=new ComboBox(), mode=new ComboBox(); CheckBox recurse=new CheckBox();
 Button start=new Button(), stop=new Button(), save=new Button(), browse=new Button(); Label status=new Label(); DataGridView grid=new DataGridView();
 BackgroundWorker worker=new BackgroundWorker(); Result result;
 public MainForm() {
  Text="FilTjek – find gamle filer"; Size=new Size(1120,760); MinimumSize=new Size(1120,700); StartPosition=FormStartPosition.CenterScreen; Font=new Font("Segoe UI",10); BackColor=Color.FromArgb(246,248,251); AutoScaleMode=AutoScaleMode.Dpi;
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),ColumnCount=1,RowCount=8}; Controls.Add(layout);
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,86)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,72)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,46)); layout.RowStyles.Add(new RowStyle(SizeType.Percent,100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
  layout.Controls.Add(new Label{Text="Find filer, der måske ikke længere bruges",Font=new Font("Segoe UI",19,FontStyle.Bold),Dock=DockStyle.Fill},0,0);
  layout.Controls.Add(new Label{Text=@"Mappe: lokal sti (C:\Data) eller netværkssti (\\server\deling)",Dock=DockStyle.Fill},0,1);
  var paths=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2}; paths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100)); paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110)); path.Dock=DockStyle.Fill; browse.Text="Vælg mappe"; browse.Dock=DockStyle.Fill; paths.Controls.Add(path); paths.Controls.Add(browse); layout.Controls.Add(paths,0,2);
  var opts=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=true};
  years.DropDownStyle=mode.DropDownStyle=ComboBoxStyle.DropDownList; years.Width=85; for(int i=1;i<=10;i++) years.Items.Add(i+" år"); years.SelectedIndex=0;
  mode.Width=320; mode.Items.AddRange(Scanner.Modes); mode.SelectedIndex=2; types.Width=145; recurse.Text="Medtag undermapper"; recurse.Checked=true; recurse.AutoSize=true;
  opts.Controls.Add(new Label{Text="Ældre end",AutoSize=true,Margin=new Padding(0,7,6,0)}); opts.Controls.Add(years); opts.Controls.Add(mode); opts.Controls.Add(recurse);
  var typeLine=new FlowLayoutPanel{Width=900,Height=34}; typeLine.Controls.Add(new Label{Text="Filtyper (valgfrit)",AutoSize=true,Margin=new Padding(0,7,6,0)}); typeLine.Controls.Add(types); typeLine.Controls.Add(new Label{Text="Fx .pdf;.docx;.xlsx — tomt felt medtager alle filer",AutoSize=true,Margin=new Padding(8,7,0,0)}); opts.Controls.Add(typeLine); layout.Controls.Add(opts,0,3);
  layout.Controls.Add(new Label{Text="Bemærk: Sidste adgang er ikke en sikker registrering af, hvornår nogen åbnede filen. Windows eller serveren kan undlade at opdatere den. Rapporten viser kandidater, ikke dokumenteret ubrugte filer. Programmet læser kun filoplysninger og sletter intet.",Dock=DockStyle.Fill,BackColor=Color.FromArgb(255,244,216),Padding=new Padding(10)},0,4);
  var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill}; start.Text="Start scanning"; stop.Text="Stop"; save.Text="Gem rapport…"; foreach(var b in new[]{start,stop,save}) { b.AutoSize=true; b.Height=34; buttons.Controls.Add(b); } stop.Enabled=save.Enabled=false; layout.Controls.Add(buttons,0,5);
  grid.Dock=DockStyle.Fill; grid.ReadOnly=true; grid.AllowUserToAddRows=false; grid.AllowUserToDeleteRows=false; grid.RowHeadersVisible=false; grid.BackgroundColor=Color.White; grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
  foreach(string s in new[]{"Filsti","Filtype","Bytes","Sidst ændret","Registreret adgang","Oprettet"}) grid.Columns.Add(s,s); grid.Columns[0].FillWeight=250; layout.Controls.Add(grid,0,6);
  status.Dock=DockStyle.Fill; status.Text="Klar. De første 500 fund vises her; rapporten indeholder alle fund."; layout.Controls.Add(status,0,7);
  browse.Click+=(s,e)=>{using(var dialog=new FolderBrowserDialog()){if(dialog.ShowDialog()==DialogResult.OK) path.Text=dialog.SelectedPath;}};
  worker.WorkerSupportsCancellation=true; worker.WorkerReportsProgress=true;
  worker.DoWork+=(s,e)=>{e.Result=Scanner.Run((Options)e.Argument,()=>worker.CancellationPending,r=>worker.ReportProgress(0,new long[]{r.Seen,r.Matches,r.Errors,r.Skipped}));};
  worker.ProgressChanged+=(s,e)=>{var v=(long[])e.UserState; status.Text=String.Format("Undersøgt: {0:N0}   •   Fund: {1:N0}   •   Fejl: {2:N0}   •   Sprunget over: {3:N0}",v[0],v[1],v[2],v[3]);};
  worker.RunWorkerCompleted+=(s,e)=>{SetBusy(false); if(e.Error!=null){status.Text="Scanning mislykkedes."; MessageBox.Show(this,e.Error.Message,"Scanning mislykkedes");return;} result=(Result)e.Result; foreach(var row in result.Preview) grid.Rows.Add(row.Take(6).Cast<object>().ToArray()); save.Enabled=true; status.Text=String.Format("{0} • {1:N0} fund / {2:N0} filer • {3:N2} GB • {4} fejl • {5} sprunget over",result.Cancelled?"Afbrudt (delvis rapport)":"Afsluttet",result.Matches,result.Seen,result.Bytes/1073741824.0,result.Errors,result.Skipped);};
  start.Click+=(s,e)=>BeginScan(); stop.Click+=(s,e)=>{worker.CancelAsync();stop.Enabled=false;status.Text="Stop anmodet. Afventer igangværende læsning; netværk kan tage tid.";}; save.Click+=(s,e)=>SaveReport();
  FormClosing+=(s,e)=>{if(worker.IsBusy){worker.CancelAsync();e.Cancel=true;status.Text="Stopper først scanningen. Luk vinduet igen, når den er stoppet.";}else Cleanup();};
 }
 void SetBusy(bool b){start.Enabled=browse.Enabled=path.Enabled=years.Enabled=mode.Enabled=types.Enabled=recurse.Enabled=!b;stop.Enabled=b;save.Enabled=!b&&result!=null;}
 void Cleanup(){if(result!=null){try{System.IO.Directory.Delete(result.Directory,true);}catch{}result=null;}}
 void BeginScan(){string p=path.Text.Trim().Trim('"'); if(!(p.StartsWith(@"\\") || (p.Length>=3&&Char.IsLetter(p[0])&&p[1]==':'&&(p[2]=='\\'||p[2]=='/')))){MessageBox.Show(this,@"Indtast en fuld sti, fx C:\Data eller \\server\deling.");return;} try{p=Path.GetFullPath(p);}catch(Exception ex){MessageBox.Show(this,ex.Message);return;} Cleanup();grid.Rows.Clear();SetBusy(true);status.Text="Starter scanning…";worker.RunWorkerAsync(new Options{Root=p,Years=years.SelectedIndex+1,Mode=mode.SelectedIndex,Recursive=recurse.Checked,Extensions=types.Text,Now=DateTime.Now});}
 void SaveReport(){using(var d=new FolderBrowserDialog{Description="Vælg, hvor rapportmappen skal gemmes"}){if(d.ShowDialog()!=DialogResult.OK)return; string destination=Path.Combine(d.SelectedPath,"FilTjek-rapport-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,6));try{System.IO.Directory.CreateDirectory(destination);foreach(string f in System.IO.Directory.GetFiles(result.Directory))File.Copy(f,Path.Combine(destination,Path.GetFileName(f)));MessageBox.Show(this,"Rapport gemt i:\r\n"+destination+"\r\n\r\nÅbn Filer.csv i Excel. Rapport.txt beskriver afgrænsning og eventuelle fejl.","Rapport gemt");}catch(Exception ex){MessageBox.Show(this,"Rapporten kunne ikke gemmes fuldt: "+ex.Message);}}}
}
public static class Program {
 [STAThread] public static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new MainForm());}
}
}
