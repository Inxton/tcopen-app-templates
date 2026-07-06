
using x_template_xPlc;
using x_template_xPlcConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TcOpen.Inxton.Input;
using Vortex.Connector;
using Vortex.Connector.ValueTypes;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;

namespace x_template_xHmi.Wpf.Data.MongoExport

{
    public class MongoExportViewModel : INotifyPropertyChanged, IDisposable
    {
        #region SETTINGS
        private static readonly string _path = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _fieldFile = "ExportFieldList";
        private static readonly string _settingsFile = "ExportSettings";
        private static readonly string _mongoExportPath = @"C:\Program Files\MongoDB\Tools\bin\mongoexport.exe";
        #endregion

        #region COMMANDS
        public RelayCommand Save { get; private set; }
        public RelayCommand Export { get; private set; }
        public RelayCommand Browse { get; private set; }
        public RelayCommand CancelExport { get; private set; }
        #endregion


        #region PRIVATE FIELDS
        private ProcessData _data;
        private PropertyInfo[] _info;
        private List<FieldItem> _searchResults = new List<FieldItem>();
        private bool _exactTime;
        private bool _exporting = false;
        private CancellationTokenSource _cts;
        private readonly object _saveLock = new object();
        private readonly DispatcherTimer _saveTimer;
        private bool _saving;

        private static List<string> _digitalInspfields = new List<string>();// { "TimeStamp", "PassTime", "FailTime", "Result", "IsExcluded", "IsByPassed", "FailureDescription", "NumberOfAllowedRetries", "RetryAttemptsCount", "ErrorCode", "RequiredStatus", "DetectedStatus" };
        private static List<string> _dataInspfields = new List<string>();// (_digitalInspfields) { "StarNotationEnabled" };
        private static List<string> _analogInspfields = new List<string>();//(_digitalInspfields) { "RequiredMin", "RequiredMax" };
        private bool disposedValue;
        #endregion


        #region CONSTRUCTOR
        public MongoExportViewModel()
        {
            Populate();

            Save = new RelayCommand(a => WriteFieldFile());

            Export = new RelayCommand(a => ExportNow());

            Browse = new RelayCommand(a => SelectOutputLocation());

            CancelExport = new RelayCommand(a => Cancel(), a => Exporting);

            _saveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            _saveTimer.Tick += async (_, __) =>
            {
                _saveTimer.Stop();

                if (_saving) return;

                try
                {
                    _saving = true;

                    await Task.Run(() => SerializeJson());
                }
                finally
                {
                    _saving = false;
                }
            };

            Application.Current.Exit -= OnExit;
            Application.Current.Exit += OnExit;

        }
        #endregion


        #region CRUD
        // read property/field named "_data" (public or non-public)
        private static List<string> GetInspectorDataObject(object inspector)
        {
            var list = new List<string>();
            object dataObj = null;
            if (inspector == null) return new List<string>();

            var t = inspector.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // Property first
            var p = t.GetProperty("_data", flags);
            if (p != null && p.CanRead)
            {
                dataObj = SafeGet(() => p.GetValue(inspector, null));
                if (dataObj != null)
                {
                    foreach (var item in dataObj.GetType().GetProperties())
                    {
                        list.Add(item.Name);
                    }
                }
            }

            return list;
        }

        private void Populate()
        {
            Model = new TreeModel();
            PopulateModel();
            _digitalInspfields = GetInspectorDataObject(new TcoInspectors.PlainTcoDigitalInspector());
            _analogInspfields = GetInspectorDataObject(new TcoInspectors.PlainTcoAnalogueInspector());
            _dataInspfields = GetInspectorDataObject(new TcoInspectors.PlainTcoDataInspector());

            HeaderItem itm = new HeaderItem();
            itm.Name = nameof(TcoInspectors.TcoDigitalInspector);


            foreach (var item in _digitalInspfields)
            {
                itm.Items.Add(new FieldItem() { AttributeName = item, Included = true });
            }

            itm.Required = true;
            // Update or add into Model
            int idx = Model.InspectorsItems.FindIndex(x => x.Name.Equals(itm.Name));
            if (idx == -1)
            {
                Model.InspectorsItems.Add(itm);
            }
            else
            {
                foreach (var it in Model.InspectorsItems[idx].Items)
                {
                    var i = itm.Items.FindIndex(x => x.AttributeName == it.AttributeName);
                    if (i != -1) itm.Items[i] = it;
                }
                itm.Required = Model.InspectorsItems[idx].Required;
                Model.InspectorsItems[idx] = itm;
            }


            itm = new HeaderItem();
            itm.Name = nameof(TcoInspectors.TcoAnalogueInspector);

            foreach (var item in _analogInspfields)
            {
                itm.Items.Add(new FieldItem() { AttributeName = item, Included = true });
            }

            itm.Required = true;

            // Update or add into Model
            idx = Model.InspectorsItems.FindIndex(x => x.Name.Equals(itm.Name));
            if (idx == -1)
            {
                Model.InspectorsItems.Add(itm);
            }
            else
            {
                foreach (var it in Model.InspectorsItems[idx].Items)
                {
                    var i = itm.Items.FindIndex(x => x.AttributeName == it.AttributeName);
                    if (i != -1) itm.Items[i] = it;
                }
                itm.Required = Model.InspectorsItems[idx].Required;
                Model.InspectorsItems[idx] = itm;
            }

            itm = new HeaderItem();
            itm.Name = nameof(TcoInspectors.TcoDataInspector);
            foreach (var item in _dataInspfields)
            {
                itm.Items.Add(new FieldItem() { AttributeName = item, Included = true });
            }

            itm.Required = true;

            // Update or add into Model
            idx = Model.InspectorsItems.FindIndex(x => x.Name.Equals(itm.Name));
            if (idx == -1)
            {
                Model.InspectorsItems.Add(itm);

            }
            else
            {
                foreach (var it in Model.InspectorsItems[idx].Items)
                {
                    var i = itm.Items.FindIndex(x => x.AttributeName == it.AttributeName);
                    if (i != -1) itm.Items[i] = it;
                }
                itm.Required = Model.InspectorsItems[idx].Required;
                Model.InspectorsItems[idx] = itm;
            }
            Model.InspectorsItems.ForEach(x => x.Subscribe());


            _data = Entry.Plc.MAIN._technology._processSettings._data;
            _info = _data.GetType().GetProperties();
            PopulateHeader(_data);
            PopulateStations();

            Model.Items.ForEach(i => i.PropertyChanged -= TreeModel_PropertyChanged);
            Model.Items.ForEach(i => i.PropertyChanged += TreeModel_PropertyChanged);

            Model.InspectorsItems.ForEach(i => i.PropertyChanged -= TreeModel_PropertyChanged);
            Model.InspectorsItems.ForEach(i => i.PropertyChanged += TreeModel_PropertyChanged);


        }
        private void WriteFieldFile()
        {
            string fields = "_EntityId" + Environment.NewLine;
            FieldItem[] attributes;

            foreach (HeaderItem item in Model.Items)
            {


                ///TcoDataInspector
                if (item.ToString().Contains("StarNotationEnabled"))
                {
                    attributes = Model.InspectorsItems.Where(ii => ii.Name.Equals("TcoDataInspector"))
                                                          .SelectMany(ii => ii.Items)
                                                          .Where(i => i.Included).ToArray();

                    fields += CheckAttributes(item, attributes);
                }
                ///TcoAnalougeInspector
                else if (item.ToString().Contains("RequiredMin"))
                {
                    attributes = Model.InspectorsItems.Where(ii => ii.Name.Equals("TcoAnalogueInspector"))
                                                          .SelectMany(ii => ii.Items)
                                                          .Where(i => i.Included).ToArray();

                    fields += CheckAttributes(item, attributes);
                }
                ///TcoDigitalInspector
                else if (item.ToString().Contains("NumberOfAllowedRetries"))
                {
                    attributes = Model.InspectorsItems.Where(ii => ii.Name.Equals("TcoDigitalInspector"))
                                                          .SelectMany(ii => ii.Items)
                                                          .Where(i => i.Included).ToArray();

                    fields += CheckAttributes(item, attributes);
                }
                else fields += item.ToString();

            }

            File.WriteAllText(_path + _fieldFile, fields);

            Console.WriteLine(fields);

            SerializeJson();
        }

        private static string CheckAttributes(HeaderItem item, FieldItem[] attributes)
        {
            string fields = string.Empty;

            string[] lines = item.ToString().Split('\n');

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.Contains("_data"))
                {
                    fields += $"{line}\n";
                    continue;
                }

                string fieldName = line.Split('.').Last();

                if (attributes.Any(a => fieldName == a.AttributeName)) fields += $"{line}\n";
            }

            return fields;
        }



        private void SerializeJson()
        {
            lock (_saveLock)
            {
                string path = _path + _settingsFile;
                string tempPath = path + ".tmp";
                string backupPath = path + ".bak";

                var obj = JsonConvert.SerializeObject(Model, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                });


                File.WriteAllText(tempPath, obj);

                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }
            }
        }
        #endregion


        #region POPULATE MODEL
        private void PopulateModel()
        {
            string path = _path + _settingsFile;
            if (!File.Exists(path)) return;

            try
            {
                var bData = File.ReadAllBytes(path);
                Model = (TreeModel)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bData), typeof(TreeModel),
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }
                );
            }
            catch ///if data read fails try backup
            {
                string backup = path + ".bak";
                if (File.Exists(backup))
                {
                    var bData = File.ReadAllBytes(backup);
                    Model = (TreeModel)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bData), typeof(TreeModel),
                        new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }
                    );
                }
            }
        }


        private void PopulateHeader(object data)
        {
            if (data == null) return;

            var info = data.GetType().GetProperties();

            // Find a property that looks like a header
            PropertyInfo headerInfo = info.FirstOrDefault(x => x.PropertyType.Name.EndsWith("Header"));
            if (headerInfo == null) return;

            object headerObj = headerInfo.GetValue(data);
            if (headerObj == null) return;

            var mainHeader = new HeaderItem
            {
                Name = headerInfo.Name
            };

            _searchResults = new List<FieldItem>();
            SearchEntity(headerObj);

            foreach (var fi in _searchResults)
            {
                mainHeader.Add(fi);
            }

            // Update or add into Model
            int idx = Model.Items.FindIndex(x => x.Name.Equals(mainHeader.Name));
            if (idx == -1)
            {
                Model.Items.Add(mainHeader);
            }
            else
            {
                foreach (var itm in Model.Items[idx].Items)
                {
                    var i = mainHeader.Items.FindIndex(x => x.AttributeName == itm.AttributeName);
                    if (i != -1) mainHeader.Items[i] = itm;
                }
                mainHeader.Required = Model.Items[idx].Required;
                Model.Items[idx] = mainHeader;
            }


        }


        private void PopulateStations()
        {
            List<PropertyInfo> stationList = _info.AsQueryable().Where(x => typeof(CUProcessDataBase).IsAssignableFrom(x.PropertyType)).ToList();

            foreach (PropertyInfo processData in stationList)
            {
                object innerObject = processData.GetValue(_data);
                if (innerObject != null)
                {
                    var instance = innerObject as CUProcessDataBase;
                    if (instance != null)
                    {
                        HeaderItem station = new HeaderItem();

                        PropertyInfo[] properties = instance.GetType().GetProperties();
                        foreach (PropertyInfo info in properties)
                        {
                            string name = info.Name;
                            object obj = info.GetValue(instance);
                            station.Name = processData.Name;

                            FieldItem item;

                            if (FindInspector(obj, out item)) { station.Add(item); continue; }
                            if (FindPrimitive(obj, out item)) { station.Add(item); continue; }

                            _searchResults = new List<FieldItem>();
                            SearchEntity(obj);///check if the object is a struct that is hidng primitives or inspectors then add them as well
                            foreach (var fi in _searchResults) station.Add(fi);

                        }

                        //foreach (var f in stationFields) station.Fields += processData.Name + ".Header" + f + '\n';

                        int stationIdx = Model.Items.FindIndex(x => x.Name.Equals(processData.Name));
                        if (stationIdx == -1) Model.Items.Add(station);///new station
                        else///update existing
                        {
                            foreach (var itm in Model.Items[stationIdx].Items)
                            {
                                var i = station.Items.FindIndex(x => x.AttributeName == itm.AttributeName);
                                if (i != -1) station.Items[i] = itm;
                            }
                            station.Required = Model.Items[stationIdx].Required;
                            Model.Items[stationIdx] = station;
                        }
                    }
                }
            }
            Model.Items.ForEach(x => x.Subscribe());/// reattach events to handlers 
        }

        /// <summary>
        /// Recursively finds Inspectors and Primitives in structures then add them to _searchResults 
        /// </summary>
        /// <param name="obj">takes any object type, but only works on IVortexObject types</param>
        private void SearchEntity(object obj)
        {
            if (obj == null) return;

            // Primary path: IVortexObject graph
            if (obj is IVortexObject vObj)
            {
                foreach (var kid in vObj.GetKids())
                    ScanCandidate(kid);
                return;
            }



            void ScanCandidate(object o)
            {
                if (o == null) return;

                // Direct hit: primitive/inspector
                if (FindInspector(o, out var item) || FindPrimitive(o, out item))
                {
                    _searchResults.Add(item);
                    return;
                }

                // Arrays of anything: dive in
                if (o is Array arr)
                {
                    for (int i = 0; i < arr.Length; i++)
                        SearchEntity(arr.GetValue(i));
                    return;
                }

                // Recurse if it’s another IVortexObject or a nested wrapper
                if (o is IVortexObject)
                {
                    SearchEntity(o);
                    return;
                }

                // If it exposes a Symbol (thus likely a Vortex element), still try to dive
                if (TryGetSymbol(o, out _))
                {
                    // Even if it's not IVortexObject, its children might be reachable via reflection
                    SearchEntity(o);
                }
            }
        }

        private static string SafeRelativePath(object obj)
        {
            if (!TryGetSymbol(obj, out var s)) return null;
            return s.Replace("MAIN._technology._processSettings._data.", "");
        }

        private static bool TryGetSymbol(object obj, out string symbol)
        {
            symbol = null;
            if (obj == null) return false;

            // Most Vortex elements (Onliners, Inspectors, IVortexObject, etc.)
            if (obj is Vortex.Connector.IVortexElement ve && !string.IsNullOrWhiteSpace(ve.Symbol))
            {
                symbol = ve.Symbol;
                return true;
            }

            // Fallback: reflect a public instance string property named "Symbol"
            var prop = obj.GetType().GetProperty("Symbol", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                symbol = prop.GetValue(obj) as string;
                if (!string.IsNullOrWhiteSpace(symbol)) return true;
            }

            return false;
        }

        private static bool FindPrimitive(object obj, out FieldItem fi)
        {
            if (obj is OnlinerBool b)
            {

                fi = new FieldItem() { AttributeName = RelativePath(b.Symbol), Field = RelativePath(b.Symbol) };
                return true;
            }

            if (obj is OnlinerReal r)
            {
                fi = new FieldItem() { AttributeName = RelativePath(r.Symbol), Field = RelativePath(r.Symbol) };
                return true;
            }

            if (obj is OnlinerInt i)
            {
                fi = new FieldItem() { AttributeName = RelativePath(i.Symbol), Field = RelativePath(i.Symbol) };
                return true;
            }

            if (obj is OnlinerSInt si)
            {
                fi = new FieldItem() { AttributeName = RelativePath(si.Symbol), Field = RelativePath(si.Symbol) };
                return true;
            }

            if (obj is OnlinerUSInt usi)
            {
                fi = new FieldItem() { AttributeName = RelativePath(usi.Symbol), Field = RelativePath(usi.Symbol) };
                return true;
            }

            if (obj is OnlinerDInt di)
            {
                fi = new FieldItem() { AttributeName = RelativePath(di.Symbol), Field = RelativePath(di.Symbol) };
                return true;
            }

            if (obj is OnlinerUDInt udi)
            {
                fi = new FieldItem() { AttributeName = RelativePath(udi.Symbol), Field = RelativePath(udi.Symbol) };
                return true;
            }

            if (obj is OnlinerString s)
            {
                fi = new FieldItem() { AttributeName = RelativePath(s.Symbol), Field = RelativePath(s.Symbol) };
                return true;
            }

            if (obj is OnlinerWString ws)
            {
                fi = new FieldItem() { AttributeName = RelativePath(ws.Symbol), Field = RelativePath(ws.Symbol) };
                return true;
            }

            if (obj is OnlinerTime t)
            {
                fi = new FieldItem() { AttributeName = RelativePath(t.Symbol), Field = RelativePath(t.Symbol) };
                return true;
            }

            if (obj is OnlinerLTime lt)
            {
                fi = new FieldItem() { AttributeName = RelativePath(lt.Symbol), Field = RelativePath(lt.Symbol) };
                return true;
            }

            if (obj is OnlinerDate d)
            {
                fi = new FieldItem() { AttributeName = RelativePath(d.Symbol), Field = RelativePath(d.Symbol) };
                return true;
            }

            if (obj is OnlinerDateTime dt)
            {
                fi = new FieldItem() { AttributeName = RelativePath(dt.Symbol), Field = RelativePath(dt.Symbol) };
                return true;
            }

            if (obj is OnlinerTimeOfDay tod)
            {
                fi = new FieldItem() { AttributeName = RelativePath(tod.Symbol), Field = RelativePath(tod.Symbol) };
                return true;
            }

            if (obj is OnlinerByte bt)
            {
                fi = new FieldItem() { AttributeName = RelativePath(bt.Symbol), Field = RelativePath(bt.Symbol) };
                return true;
            }

            if (obj is OnlinerWord w)
            {
                fi = new FieldItem() { AttributeName = RelativePath(w.Symbol), Field = RelativePath(w.Symbol) };
                return true;
            }

            if (obj is OnlinerDWord dw)
            {
                fi = new FieldItem() { AttributeName = RelativePath(dw.Symbol), Field = RelativePath(dw.Symbol) };
                return true;
            }

            if (obj is OnlinerLWord lw)
            {
                fi = new FieldItem() { AttributeName = RelativePath(lw.Symbol), Field = RelativePath(lw.Symbol) };
                return true;
            }
            fi = new FieldItem();
            return false;
        }


        private static bool FindInspector(object obj, out FieldItem fi)
        {
            string inspectorFields = string.Empty;
            if (obj is TcoInspectors.TcoDataInspector dataInspector)
            {
                foreach (var field in _dataInspfields)
                {
                    inspectorFields += RelativePath(dataInspector.Symbol) + "._data." + field + '\n';
                }

                fi = new FieldItem() { AttributeName = RelativePath(dataInspector.Symbol), Field = inspectorFields };
                return true;
            }

            if (obj is TcoInspectors.TcoDigitalInspector logicInspector)
            {
                foreach (var field in _digitalInspfields)
                {
                    inspectorFields += RelativePath(logicInspector.Symbol) + "._data." + field + '\n';
                }
                fi = new FieldItem() { AttributeName = RelativePath(logicInspector.Symbol), Field = inspectorFields };
                return true;
            }

            if (obj is TcoInspectors.TcoAnalogueInspector analougeInspector)
            {
                inspectorFields = string.Empty;
                foreach (var field in _analogInspfields)
                {
                    inspectorFields += RelativePath(analougeInspector.Symbol) + "._data." + field + '\n';
                }
                fi = new FieldItem() { AttributeName = RelativePath(analougeInspector.Symbol), Field = inspectorFields };
                return true; ;
            }
            fi = new FieldItem();
            return false;
        }

        public static string RelativePath(string s)
        {
            return s.Replace("MAIN._technology._processSettings._data.", "");
        }
        #endregion


        #region EXPORT
        private async void ExportNow() { await ExportAsync(); }

        private async Task ExportAsync()
        {
            Exporting = true;

            TaskDialog td = new TaskDialog();
            //td.Opened += new EventHandler(td_Opened);
            //td.OwnerWindowHandle = this.Handle;
            TaskDialogStandardButtons button = TaskDialogStandardButtons.Ok;
            td.Caption = Properties.strings.MongoExport;
            td.StandardButtons = button;
            if (!File.Exists(_mongoExportPath))
            {
                td.Icon = TaskDialogStandardIcon.Error;
                td.InstructionText = Properties.strings.Error;
                td.Text = string.Format(
                                Properties.strings.MongoExportPathErrorMsg,
                                _mongoExportPath);
                td.Show();
                Exporting = false;
                return;
            }
            WriteFieldFile();


            string date = SelectedFromDate == SelectedToDate
                                        ? SelectedFromDate.ToString("yyyy.MM.dd")
                                        : $"{SelectedFromDate.ToString("yyyy.MM.dd")}_{SelectedToDate.ToString("yyyy.MM.dd")}";


            DateTime toQueryDate = SelectedFromDate != SelectedToDate
                                                            ? SelectedToDate
                                                            : SelectedToDate.AddDays(1);


            string outputFile = $"\\{Entry.Settings.DbName}_{date}.csv";

            JObject dateFilter = new JObject
            {
                {
                    "_Modified", new JObject
                    {
                        { "$gte", new JObject { { "$date", SelectedFromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } } },
                        { "$lt", new JObject { { "$date", toQueryDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") } } }
                    }
                }
            };

            string host = new Func<string>(() => { var a = Entry.Settings.GetConnectionString().Split('/'); return a.Last(); })();/// remove "mongodb://"

            var psi = new ProcessStartInfo()
            {
                FileName = _mongoExportPath,
                Arguments = $"--host {host} " +
                $"--db {Entry.Settings.DbName} " +
                $"--collection Traceability " +
                $"--type=csv " +
                $"--query \"{dateFilter.ToString().Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\"", "\\\"")}\" " +
                $"--limit 0 " +
                $"--fieldFile {_path + _fieldFile} " +
                $"--out {OutputLocation + outputFile}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            string errorOutput = string.Empty;

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            Task<string> stderrTask;

            try
            {
                await Task.Run(() =>
                {
                    using (var process = new Process { StartInfo = psi })
                    {
                        try
                        {
                            process.Start();

                            stderrTask = Task.Run(() => process.StandardError.ReadToEnd());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Process failed to start:");
                            Console.WriteLine(ex.ToString());

                            td.Icon = TaskDialogStandardIcon.Error;
                            td.InstructionText = Properties.strings.ExportError;
                            td.Text = string.Format(
                                Properties.strings.StartingProcessFailed,
                                ex.Message);
                            td.Show();
                            Exporting = false;
                            return;
                        }


                        while (!process.HasExited)
                        {
                            if (token.IsCancellationRequested)
                            {
                                try { process.Kill(); } catch { }
                                token.ThrowIfCancellationRequested();
                            }

                            Thread.Sleep(100); ///Polling delay to reduce load on CPU
                        }

                        Task.WaitAll(stderrTask);///MongoExport sends all its output to StandardError

                        errorOutput = stderrTask.Result;

                    }
                }, token);
            }
            catch (OperationCanceledException)
            {
                td.Icon = TaskDialogStandardIcon.Warning;
                td.InstructionText = Properties.strings.ExportCancelled;
                td.Text = Properties.strings.TheExportOperationWasCancelled;
                Exporting = false;
                td.Show();
                return;
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }


            Console.WriteLine($"Mongo export args: {psi.Arguments}");

            string[] msg = errorOutput.Split('\t');

            bool error = !msg.Last().StartsWith("exported");
            td.Icon = error ? TaskDialogStandardIcon.Error : TaskDialogStandardIcon.Information;

            td.InstructionText = error
                                ? Properties.strings.Error
                                : (msg.Length > 1 ? msg[1].Split('\n')[0] : string.Empty);

            string infoText = string.Empty;
            if (error)
            {
                foreach (var line in msg)
                {
                    if (line.StartsWith("error"))
                    {
                        infoText += line.Split('\n')[0];
                    }
                }
            }
            if (infoText.Length == 0) infoText = msg.Last();
            td.Text = infoText;


            Exporting = false;
            TaskDialogResult res = td.Show();
        }


        private void SelectOutputLocation()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = OutputLocation != string.Empty ? OutputLocation : @"C:\Users",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputLocation = dialog.FileName;
                SerializeJson();
            }


        }
        #endregion



        protected void ResetTime()
        {
            SelectedFromDate = SelectedFromDate.Date;
            SelectedToDate = SelectedToDate.Date;
        }

        private static object SafeGet(Func<object> getter)
        {
            try { return getter(); } catch { return null; }
        }

        private void Cancel()
        {
            _cts?.Cancel();
        }


        #region PUBLIC PROPERTIES
        public TreeModel Model { get; set; }
        public DateTime SelectedFromDate { get; set; } = DateTime.Today;
        public DateTime SelectedToDate { get; set; } = DateTime.Today;

        public bool Exporting
        {
            get => _exporting;
            set
            {
                _exporting = value; OnPropertyChanged(nameof(Exporting));
                CancelExport?.RaiseCanExecuteChanged();
            }
        }

        public bool ExactTime
        {
            get => _exactTime;
            set
            {
                if (_exactTime == value) return;
                if (!value) ResetTime();

                _exactTime = value;
                OnPropertyChanged(nameof(ExactTime));
                OnPropertyChanged(nameof(SelectedFromDate));
                OnPropertyChanged(nameof(SelectedToDate));
            }
        }

        public string OutputLocation
        {
            get => Model.OutputLocation;
            set { Model.OutputLocation = value; NotifyPropertyChanged(nameof(OutputLocation)); }
        }
        #endregion

        #region INOTIFYPROPERTYCHANGED
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region EVENT HANDLERS
        private void TreeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _saveTimer.Stop();

            if (!_saving)
            {
                SerializeJson();
            }
        }
        #endregion


        #region IDISPOSABLE
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _saveTimer?.Stop();
                    _cts?.Dispose();
                    Application.Current.Exit -= OnExit;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
