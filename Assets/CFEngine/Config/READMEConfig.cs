namespace CrystalFrost.Config

/// <summary>
/// This is a description of how to define configuration classes and
/// how to use them.
/// </summary>
{
    /// <summary>
    /// Configuration in CrystalFrost is defined by creating classes for each
    /// of the various subsystems. The classes are defined in separate files
    /// usually in the Assets/CFEngine/Config folder. Each class has the form
    /// given below.
    /// 
    /// The class contains a constant string called subsectionName which is
    /// used to qualify the configuration parameters. This is followed by
    /// parameter defintions with 'get' and 'set' accessors and an assignment
    /// of the default value for the parameter.
    /// 
    /// Once the subsection parameter class is defined, create its instance
    /// by adding it to the IServiceCollection in the RegisterComponents function in
    /// `Assets/CFEngine/Services.cs`. This creates a singleton instance of
    /// of the class and fills the parameters with values from the config file,
    /// command line, and environment variables.
    /// <code>
    ///     _serviceCollection.Configure<READMEConfig>(Services._configRoot.GetSection(READMEConfig.subsectionName));
    /// </code>
    /// 
    /// Command line values are specified with the form:
    /// <code>
    ///      --subsectionName:parameterName=value (Linux)
    ///      /subsectionName:parameterName=value (Windows)
    /// </code>
    /// Environment variables are specified with the form:
    /// <code>
    ///     CF_subsectionName:parameterName=value
    ///     CF_subsectionName__parameterName=value
    /// </code>
    /// NOTE THAT the subsection seperator is colon for command line and can be double underscore
    /// for environment variables (since colons aren't good in some OS's ).
    ///     
    /// The config file is a JSON file named `cf-config.json` in the root.
    /// 
    /// To reference the configuration parameters, fetch the singleton instance
    ///     var readmeConfig = serviceProvider.GetService<IOptions<READMEConfig>>().Value;
    /// NOTE THAT THIS GETS THE ".Value" from the IOptions interface to the configuation
    /// instance. IOptions has a bunch of features like getting events when config
    /// values change and such. The ".Value" is the actual instance of the configuration.
    /// 
    /// To change a configuration parameter at runtime, fetch the singleton instance
    /// and assign a new value to the parameter.
    /// <code>
    ///     var readmeConfig = serviceProvider.GetService<IOptions<READMEConfig>>().Value;
    ///     readmeConfig.StringParam = "new value";
    /// </code>
    ///  
    /// When using Dependency Injection, the configuration parameters can be injected:
    /// <code>
    ///    public class SomeClass {
    ///        private ILogger<SomeClass> _log;
    ///        private IOptions<READMEConfig> _readmeConfig;
    ///        public SomeClass(ILogger<SomeClass> log,
    ///                         ...,
    ///                         IOptions<READMEConfig> readmeConfig) {
    ///            _log = log;
    ///            _readmeConfig = readmeConfig.Value;  // NOTE THE .Value
    ///        }
    ///    }
    /// </code>
    /// 
    /// For a concrete example, see the MeshConfig class in Assets/CFEngine/Config/MeshConfig.cs
    /// and its references that uses both the configuration and Dependency Injection.
    /// 
    /// </summary>
    public class READMEConfig
    {
        public const string subsectionName = "READMEConfig";

        // example of a string parameter
        public string StringParam { get; set; } = "default value";
    }
}
