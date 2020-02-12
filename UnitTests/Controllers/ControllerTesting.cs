using Moq;
using Moq.AutoMock;
using System;

namespace UnitTests.Controllers
{

    /// <summary>
    /// A public class 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ControllerTesting<T> : IDisposable where T : class
    {

        /// <summary>
        /// Whether to have the service provider return a copy of the predefined instance or 
        /// </summary>
        internal bool StaticContext {
            get {
                return _staticContext;
            }
            set {
                _staticContext = value;
                this.InstantiateMockController(Controller);
            }
        }

        private bool _staticContext = true;

        /// <summary>
        /// Default constructor for the controller testing class. Will call ControllerTesting.InstantiateMockController() to instantiate a new mock controller.
        /// </summary>
        public ControllerTesting()
        {
            this.InstantiateMockController(this.CreateController());
        }

        /// <summary>
        /// Default constructor for the controller testing class. Will call ControllerTesting.InstantiateMockController(T Predefined) to instantiate a new mock controller.
        /// </summary>
        public ControllerTesting(T Predefined)
        {
            this.InstantiateMockController(Predefined);
        }

        public ControllerTesting(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets an instance of the controlled, whether it's the same instance or not depends on whether StaticContext is true or false.
        /// </summary>
        internal T Controller => (T)this.ServiceProvider.GetService(typeof(T));

        /// <summary>
        /// The service to return a new instance of a controller every time, so that we can delete things without worrying to add them back.
        /// </summary>
        private IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Method for constructing the testing controller. Override to change it from default.
        /// </summary>
        protected abstract T CreateController();

        /// <summary>
        /// Instantiate's a predefined mock controller from the parameter
        /// </summary>
        /// <param name="Predefined">A predefined instance of the mock controller</param>
        protected void InstantiateMockController(T Predefined)
        {

            if (StaticContext)
            {

                // Return a reference TO the predefined

                var ServiceProviderMock = new Mock<IServiceProvider>();
                ServiceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(Predefined);

                this.ServiceProvider = ServiceProviderMock.Object;

            }
            else
            {
                var ServiceProviderMock = new Mock<IServiceProvider>();
                ServiceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(() =>
                {

                    // Return a new T type that has the same properties as the predefined

                    var NewInstance = new AutoMocker().CreateInstance<T>();
                    this.CopyProperties(Predefined, NewInstance);

                    return NewInstance;

                });

                this.ServiceProvider = ServiceProviderMock.Object;

            }

        }

        private void CopyProperties(T From, T To)
        {

            var FromProperties = From.GetType().GetProperties();

            for (var i = 0; i < FromProperties.Length; i++)
            {

                var FromField = FromProperties[i].GetValue(From);

                if (FromField != null && FromProperties[i].SetMethod != null)
                    FromProperties[i].SetValue(To, FromField);

            }

        }

        void IDisposable.Dispose()
        {

            // Dispose the static object if it's IDisposable
            if (StaticContext && this.ServiceProvider.GetService(typeof(T)) is IDisposable)
                (this.ServiceProvider.GetService(typeof(T)) as IDisposable)?.Dispose();

        }
    }
}
