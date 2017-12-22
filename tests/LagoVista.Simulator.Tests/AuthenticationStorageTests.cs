using LagoVista.Client.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Tests
{
    [TestClass]
    public class AuthenticationStorageTests
    {
        Mock<ISecureStorage> _secureStorage = new Mock<ISecureStorage>();
        Mock<INetworkService> _networkService = new Mock<INetworkService>();
        Mock<IPopupServices> _popupService = new Mock<IPopupServices>();
        Mock<IViewModelNavigation> _vmNavigation = new Mock<IViewModelNavigation>();
        Mock<IRestClient> _restClient = new Mock<IRestClient>();
        Mock<ILogger> _logger = new Mock<ILogger>();

        [TestInitialize]
        public void Init()
        {
            SLWIOC.RegisterSingleton<INetworkService>(_networkService.Object);
            SLWIOC.RegisterSingleton<IPopupServices>(_popupService.Object);
            SLWIOC.RegisterSingleton<IViewModelNavigation>(_vmNavigation.Object);
            SLWIOC.RegisterSingleton<ILogger>(_logger.Object);
        }

        [TestMethod]
        public async Task Password()
        {
            _networkService.Setup(nets => nets.IsInternetConnected).Returns(true);
            _restClient.Setup(rst => rst.GetAsync<DetailResponse<LagoVista.IoT.Simulator.Admin.Models.Simulator>>(It.IsAny<string>(), new CancellationTokenSource())).ReturnsAsync(new LagoVista.Core.Validation.InvokeResult<DetailResponse<IoT.Simulator.Admin.Models.Simulator>>()
            {
                Result = DetailResponse<IoT.Simulator.Admin.Models.Simulator>.Create(new IoT.Simulator.Admin.Models.Simulator()
                {
                    CredentialStorage = LagoVista.Core.Models.EntityHeader<IoT.Simulator.Admin.Models.CredentialsStorage>.Create(IoT.Simulator.Admin.Models.CredentialsStorage.InCloud)
                })
            });

            var vm = new LagoVista.Simulator.Core.ViewModels.Simulator.SimulatorViewModel(_secureStorage.Object);

            await vm.InitAsync();
        }
    }
}
