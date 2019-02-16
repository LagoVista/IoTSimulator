using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorNetworkViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.SimulatorNetwork>
    {
        public SimulatorNetworkViewModel()
        {
            ShowSettingsCommand = new RelayCommand(ShowSettings);
        }

        private void ShowSettings()
        {
            if(SettingsVisible)
            {
                SettingsVisible = false;
                SettingsToggleContent = "Show Settings";
                return;
            }

            EntityHeader sysUser = null;

            switch (SLWIOC.Get<IAppConfig>().Environment)
            {
                case Environments.LocalDevelopment:
                    sysUser = EntityHeader.Create("993767B144AE4EDB815B2384485AF3F8", "SimNetworkUser");
                    break;
                case Environments.Development:
                    sysUser = EntityHeader.Create("423AA7EC788C49FF8BC6EC1E66A0CABF", "SimNetworkUser");
                    break;
                case Environments.Testing:
                    sysUser = EntityHeader.Create("3FD0B5F091E24EAB9EC3D7AC29997468", "SimNetworkUser");
                    break;
                default:
                    sysUser = EntityHeader.Create("F2135E2A9BC44A0D-8A7DB1C4886848AA", "SimNetworkUser");
                    break;
            }

            EntityHeader org = Model.OwnerOrganization;

            var settings = new StringBuilder();
            settings.Append($"-e Id={Model.Id} ");
            settings.Append($"-e AccessKey={Model.SharedAccessKey1} ");
            settings.Append($"-e OrgName={org.Text} ");
            settings.Append($"-e OrgId={org.Id} ");
            settings.Append($"-e UserId={sysUser.Id} ");
            settings.Append($"-e UserName={sysUser.Text} ");
            DockerString = settings.ToString();
            SettingsVisible = true;

            settings = new StringBuilder();
            settings.Append($"-id={Model.Id} ");
            settings.Append($"-key={Model.SharedAccessKey1} ");
            settings.Append($"-org=\"{org.Text}\" ");
            settings.Append($"-orgid={org.Id} ");
            settings.Append($"-uid={sysUser.Id} ");
            settings.Append($"-usrname={sysUser.Text} ");
            ConsoleString = settings.ToString();
            SettingsVisible = true;

            SettingsToggleContent = "Hide Settings";
        }


        public override async Task<InvokeResult> SaveRecordAsync()
        {
            return await PerformNetworkOperation(async () =>
            {
                if (LaunchArgs.LaunchType == LaunchTypes.Create)
                {
                    return await FormRestClient.AddAsync("/api/simulator/network", this.Model);
                }
                else
                {
                    return await FormRestClient.UpdateAsync("/api/simulator/network", this.Model);
                }
            });
        }

        protected override string GetRequestUri()
        {
            return this.LaunchArgs.LaunchType == LaunchTypes.Edit ? $"/api/simulator/network/{LaunchArgs.ChildId}" : "/api/simulator/network/factory";
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Key).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;

            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.SharedAccessKey1));
            form.AddViewCell(nameof(Model.SharedAccessKey2));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.Description));

            form.AddChildList<SimulatorInstanceViewModel>(nameof(Model.Simulators), Model.Simulators);
        }

        public RelayCommand ShowSettingsCommand { get; }

        private bool _settingsVisible;
        public bool SettingsVisible
        {
            get { return _settingsVisible; }
            set { Set(ref _settingsVisible, value); }
        }

        private string _dockerString;
        public string DockerString
        {
            get { return _dockerString; }
            set { Set(ref _dockerString, value); }
        }

        private string _consoleString;
        public string ConsoleString
        {
            get { return _consoleString; }
            set { Set(ref _consoleString, value); }
        }

        private string _settingsToggleContent = "Show Settings";
        public string SettingsToggleContent
        {
            get { return _settingsToggleContent; }
            set { Set(ref _settingsToggleContent, value); }
        }

    }
}
