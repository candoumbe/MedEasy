﻿#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Extensions.Hosting;
#endif
#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Hosting.Internal; 
#else
using Microsoft.Extensions.Hosting.Internal;
#endif
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Mvc.ViewComponents;
#endif
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MedEasy.IntegrationTests.Core
{
    /// <summary>
    /// A test fixture which hosts the target project (project we wish to test) in an in-memory server.
    /// </summary>
    /// <typeparam name="TStartup">Target project's startup type</typeparam>
    public abstract class BaseTestFixture<TStartup> : IDisposable where TStartup : class
    {
        protected BaseTestFixture()
        {
        }

        public TestServer Server { get; private set; }

        /// <summary>
        /// Initialize data to use during the fixture lifetime.
        /// </summary>
        /// <param name="relativeTargetProjectParentDir">Path to the directory of the project under test. </param>
        /// <param name="environmentName">Name of the environment to emulate</param>
        /// <param name="applicationName">Name of the application</param>
        /// 
        /// <exception cref="DirectoryNotFoundException"><paramref name="relativeTargetProjectParentDir"/> doesnot contain</exception>
        public virtual void Initialize(string relativeTargetProjectParentDir, string environmentName, string applicationName) => Initialize<TStartup>(relativeTargetProjectParentDir, environmentName, applicationName);

        /// <summary>
        /// Initialize data to use during the fixture lifetime.
        /// </summary>
        /// <typeparam name="TStart">Type of the startup class</typeparam>
        /// <param name="relativeTargetProjectParentDir">Path to the directory of the project under test. </param>
        /// <param name="environmentName">Name of the environment to emulate</param>
        /// <param name="applicationName">Name of the application</param>
        /// 
        /// <exception cref="DirectoryNotFoundException"><paramref name="relativeTargetProjectParentDir"/> doesnot contain</exception>
        public virtual void Initialize<TStart>(string relativeTargetProjectParentDir, string environmentName, string applicationName)
        {
            Assembly startupAssembly = typeof(TStart).GetTypeInfo().Assembly;
            string contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);

#if !NETCOREAPP3_0
            IHostingEnvironment env = new HostingEnvironment
#else
            IHostEnvironment env = new HostingEnvironment
#endif
            {
                ContentRootPath = contentRoot,
                EnvironmentName = environmentName,
                ApplicationName = applicationName,
                ContentRootFileProvider = new NullFileProvider(),
            };
#if !NETCOREAPP3_0

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables()
                .Build();

            IWebHostBuilder builder = new WebHostBuilder() 
                .UseConfiguration(configuration)
                .UseEnvironment(env.EnvironmentName)
                .UseStartup(typeof(TStart))
                .ConfigureServices(InitializeServices)
                .ConfigureServices(services => services.AddSingleton(env));
            Server = new TestServer(builder)
            {
                BaseAddress = new Uri($"http://locahost/{applicationName}")
            };
#else

            IHostBuilder builder = new HostBuilder()
                .ConfigureHostConfiguration(builder =>

                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()




                );
            Server = builder.Build().GetTestServer();
            Server.BaseAddress = new Uri($"http://locahost/{applicationName}");
#endif

        }
    

        public void Dispose() => Server?.Dispose();

        protected virtual void InitializeServices(IServiceCollection services) => InitializeServices<TStartup>(services);

        protected virtual void InitializeServices<TStart>(IServiceCollection services)
        {
            Assembly startupAssembly = typeof(TStart).GetTypeInfo().Assembly;

            // Inject a custom application part manager. 
            // Overrides AddMvcCore() because it uses TryAdd().
            ApplicationPartManager manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
            manager.FeatureProviders.Add(new ControllerFeatureProvider());
#if !NETCOREAPP3_0
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider()); 
#endif

            services.AddSingleton(manager);
        }

        /// <summary>
        /// Gets the full path to the target project that we wish to test
        /// </summary>
        /// <param name="projectRelativePath">
        /// The parent directory of the target project.
        /// e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        private static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            string projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            string applicationBasePath = AppContext.BaseDirectory;

            // Find the path to the target project
            DirectoryInfo directoryInfo = new DirectoryInfo(applicationBasePath);
            string[] directoryParts = projectRelativePath.Split(Path.DirectorySeparatorChar);
            int goToParentDirectory = 0;
            foreach (string part in directoryParts)
            {
                switch (part)
                {
                    case "..":
                        directoryInfo = directoryInfo.Parent;
                        goToParentDirectory++;
                        break;
                    case ".": break;
                    case null: break;
                    default:
                        string[] oldPath = directoryInfo.FullName.Split(Path.DirectorySeparatorChar);
                        string newPath = string.Join(Path.DirectorySeparatorChar, oldPath.Take(oldPath.Length - 1));
                        directoryInfo = new DirectoryInfo(Path.Combine(newPath, part));
                        break;
                }
            }
            projectRelativePath = string.Join(Path.DirectorySeparatorChar, directoryParts.Skip(goToParentDirectory));

            string projectDirectoryPath = null;
            bool projectDirectoryFound = false;
            do
            {
                directoryInfo = directoryInfo.Parent;
                DirectoryInfo projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
                if (projectDirectoryInfo.Exists)
                {
                    FileInfo projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                    if (projectFileInfo.Exists)
                    {
                        projectDirectoryPath = Path.Combine(projectDirectoryInfo.FullName, projectName);
                        projectDirectoryFound = true;
                    }
                }
            }
            while (directoryInfo.Parent != null && !projectDirectoryFound);

            if (!projectDirectoryFound)
            {
                throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
            }

            return projectDirectoryPath;
        }
    }
}
