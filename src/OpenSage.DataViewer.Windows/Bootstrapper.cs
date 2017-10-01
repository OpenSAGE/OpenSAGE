using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer
{
    public class Bootstrapper : BootstrapperBase
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private GameService _gameService;
        private SimpleContainer container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager();
            _gameService = new GameService();

            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Instance(_graphicsDeviceManager);
            container.Instance(_gameService);

            container.PerRequest<ShellViewModel>();

            var originalLocateTypeForModelType = ViewLocator.LocateTypeForModelType;
            ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
            {
                return originalLocateTypeForModelType(modelType, displayLocation, context)
                    ?? originalLocateTypeForModelType(modelType.BaseType, displayLocation, context);
            };
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            _gameService.Dispose();
            _graphicsDeviceManager.Dispose();

            base.OnExit(sender, e);
        }
    }
}
