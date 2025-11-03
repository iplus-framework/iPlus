using gip.core.layoutengine.avui;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.iplus.client.avui.Views
{
    [DataContract]
    public class Settings : ReactiveObject
    {
        private string _UserName;
        private string _Password;
        private bool _CtrlPressed;
        private bool _F1Pressed;
        private eWpfTheme _WPFTheme;

        public Settings()
        {
        }

        [DataMember]
        public string UserName
        {
            get => _UserName;
            set => this.RaiseAndSetIfChanged(ref _UserName, value);
        }

#if DEBUG
        [DataMember]
#else
        [IgnoreDataMember]
#endif
        public string Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }

        [IgnoreDataMember]
        public bool CtrlPressed
        {
            get => _CtrlPressed;
            set => this.RaiseAndSetIfChanged(ref _CtrlPressed, value);
        }

        [IgnoreDataMember]
        public bool F1Pressed
        {
            get => _F1Pressed;
            set => this.RaiseAndSetIfChanged(ref _F1Pressed, value);
        }

        [DataMember]
        public eWpfTheme WPFTheme
        {
            get => _WPFTheme;
            set => this.RaiseAndSetIfChanged(ref _WPFTheme, value);
        }
    }

    public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
    {
        private readonly string _file;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public NewtonsoftJsonSuspensionDriver(string file) => _file = file;

        public IObservable<Unit> InvalidateState()
        {
            if (File.Exists(_file))
                File.Delete(_file);
            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState()
        {
            var lines = File.ReadAllText(_file);
            var state = JsonConvert.DeserializeObject<object>(lines, _settings);
            return Observable.Return(state);
        }

        public IObservable<Unit> SaveState(object state)
        {
            var lines = JsonConvert.SerializeObject(state, _settings);
            File.WriteAllText(_file, lines);
            return Observable.Return(Unit.Default);
        }
    }
}
