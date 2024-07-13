using System;
using System.Collections;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using UnityEngine;
using UnityEngine.TestTools;

using NUnit.Framework;

using CrystalFrost.Config;
using CrystalFrost;

namespace CrystalFrostEngine.Tests
{
    public class ConfigRoot_Test
    {
        [Test]
        public void GridConfig_Test_Defaults()
        {
            var gridConfig = new GridConfig();
            string defaultLoginURI = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            Assert.AreEqual(defaultLoginURI, gridConfig.LoginURI);
        }

        [Test]
        public void GridConfig_Test_IOptions()
        {
            // slight hack since someone may have already set the environment variable
            string defaultLoginURI = Environment.GetEnvironmentVariable("CF_Grid__LoginURI")
                        ?? "https://login.agni.lindenlab.com/cgi-bin/login.cgi";

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(defaultLoginURI, gridConfig.LoginURI);
        }

        [Test]
        public void GridConfig_Test_Environment()
        {
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";
            Environment.SetEnvironmentVariable("CF_Grid:LoginURI", differentLoginURI);

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs);

            // Remove the variable from the environment or later tests will fail.
            Environment.SetEnvironmentVariable("CF_Grid:LoginURI", null);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(differentLoginURI, gridConfig.LoginURI);
        }

        [Test]
        public void GridConfig_Test_CommandLine_Linux()
        {
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";
            string[] args1 = { "--Grid:LoginURI", differentLoginURI };

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs, args1);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(differentLoginURI, gridConfig.LoginURI);
        }
        [Test]
        public void GridConfig_Test_CommandLine_Win()
        {
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";
            string[] args1 = { "/Grid:LoginURI", differentLoginURI };

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs, args1);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(differentLoginURI, gridConfig.LoginURI);
        }
        [Test]
        public void GridConfig_Test_CommandLine_Equals()
        {
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";
            string[] args1 = { $"Grid:LoginURI={differentLoginURI}" };

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs, args1);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(differentLoginURI, gridConfig.LoginURI);
        }
        [Test]
        public void GridConfig_Test_CommandLine_Multiple()
        {
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";
            string[] args1 = { "--Grid:LoginURI", differentLoginURI, "--Code:UseNewObjectGraph", "true" };

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs, args1);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            var codeConfig = serviceProvider.GetService<IOptions<CodeConfig>>().Value;

            Assert.AreEqual(differentLoginURI, gridConfig.LoginURI);
            Assert.AreEqual(true, codeConfig.UseNewObjectGraph);
        }

        [Test]
        public void GridConfig_Test_Runtime()
        {
            // slight hack since someone may have already set the environment variable
            string defaultLoginURI = Environment.GetEnvironmentVariable("CF_Grid__LoginURI")
                        ?? "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            string differentLoginURI = "http://grid.wolfterritories.org:8002/";

            SetupConfigAndServiceProvider(out IServiceProvider serviceProvider, out IConfigurationRoot configs);

            var gridConfig = serviceProvider.GetService<IOptions<GridConfig>>().Value;
            Assert.AreEqual(defaultLoginURI, gridConfig.LoginURI);

            gridConfig.LoginURI = differentLoginURI;
            
            var gridConfig2 = serviceProvider.GetService<IOptions<GridConfig>>().Value;

            Assert.AreEqual(differentLoginURI, gridConfig2.LoginURI);
        }

        /// <summary>
        /// Duplication of code from Assets/CFEngine/Services.cs that just sets up the configuration
        /// parameters.
        /// </summary>
        /// <param name="pSP">out parameter returning the created service provider</param>
        /// <param name="pCRoot">out parameter returning the created IConfigurationRoot</param>
        /// <param name="pArgs">optional list of args to pretend came from  the command line</param>
        public static void SetupConfigAndServiceProvider(out IServiceProvider pSP, out IConfigurationRoot pCRoot, string[] pArgs = null)
        {
            string[] args = pArgs ?? new string[0];

            IServiceCollection _serviceCollection = new ServiceCollection();

            var configs = new ConfigurationBuilder()
                .AddInMemoryCollection(LoggingConfig.DefaultValues)
                // .AddJsonFile("cf-config.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("CF_")
                .AddCommandLine(args)
                .Build();
             
            // Add configuration service for those who might need to change things
            _serviceCollection.AddSingleton<IConfigurationRoot>(configs);

            // Add IOptions config for the individual config sections
            // This will allow someone to inject IOptions<CodeConfig> to get the config params.
            //     Using the IOptions interface allows the config to be changed at runtime
            //     (see IOptionsSnapshot for more info).
            _serviceCollection.Configure<GridConfig>(configs.GetSection(GridConfig.subsectionName));
            _serviceCollection.Configure<CodeConfig>(configs.GetSection(CodeConfig.subsectionName));
            _serviceCollection.Configure<MeshConfig>(configs.GetSection(MeshConfig.subsectionName));
            _serviceCollection.Configure<TextureConfig>(configs.GetSection(TextureConfig.subsectionName));
            _serviceCollection.Configure<LoggingConfig>(configs.GetSection(LoggingConfig.subsectionName));

            pSP = _serviceCollection.BuildServiceProvider();
            pCRoot = configs;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame
        [UnityTest]
        public IEnumerator ConfigRoot_TestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}