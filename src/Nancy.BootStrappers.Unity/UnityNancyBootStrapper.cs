﻿namespace Nancy.Bootstrappers.Unity
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Practices.Unity;
    using ModelBinding;
    using Nancy.Bootstrapper;
    using Nancy.ViewEngines;

    public class UnityNancyBootstrapper : NancyBootstrapperBase<IUnityContainer>,
                                          INancyBootstrapperPerRequestRegistration<IUnityContainer>,
                                          INancyModuleCatalog
    {
        protected IUnityContainer unityContainer;

        /// <summary>
        ///  Resolve INancyEngine
        /// </summary>
        /// <returns>INancyEngine implementation</returns>
        protected override INancyEngine GetEngineInternal()
        {
            return unityContainer.Resolve<INancyEngine>();
        }

        protected override void RegisterViewEngines(IUnityContainer container, IEnumerable<Type> viewEngineTypes)
        {
            foreach (var viewEngineType in viewEngineTypes)
            {
                unityContainer.RegisterType(typeof(IViewEngine), viewEngineType, new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        ///   Create a default, unconfigured, container
        /// </summary>
        /// <returns>Container</returns>
        protected override IUnityContainer CreateContainer()
        {
            unityContainer = new UnityContainer();
            return unityContainer;
        }

        /// <summary>
        /// Get the moduleKey generator
        /// </summary>
        /// <returns>IModuleKeyGenerator instance</returns>
        protected sealed override IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return unityContainer.Resolve<IModuleKeyGenerator>();
        }

        protected override void RegisterRootPathProvider(IUnityContainer container, Type rootPathProviderType)
        {
            unityContainer.RegisterType(typeof(IRootPathProvider), rootPathProviderType, new ContainerControlledLifetimeManager());
        }

        protected override void RegisterViewSourceProviders(IUnityContainer container, IEnumerable<Type> viewSourceProviderTypes)
        {
            foreach (var viewSourceProvider in viewSourceProviderTypes)
            {
                unityContainer.RegisterType(typeof(IViewSourceProvider), viewSourceProvider, new ContainerControlledLifetimeManager());
            }
        }

        protected override void RegisterModelBinders(IUnityContainer container, IEnumerable<Type> modelBinderTypes)
        {
            foreach (var modelBinder in modelBinderTypes)
            {
                unityContainer.RegisterType(typeof(IModelBinder), modelBinder, new ContainerControlledLifetimeManager());
            }
        }

        protected override void RegisterTypeConverters(IUnityContainer container, IEnumerable<Type> typeConverterTypes)
        {
            foreach (var typeConverter in typeConverterTypes)
            {
                unityContainer.RegisterType(typeof(ITypeConverter), typeConverter, new ContainerControlledLifetimeManager());
            }
        }

        protected override void RegisterBodyDeserializers(IUnityContainer container, IEnumerable<Type> bodyDeserializerTypes)
        {
            foreach (var bodyDeserializer in bodyDeserializerTypes)
            {
                unityContainer.RegisterType(typeof(IBodyDeserializer), bodyDeserializer, new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        ///   Register the given module types into the container
        /// </summary>
        /// <param name = "moduleRegistrations">NancyModule types</param>
        protected override void RegisterModules(IEnumerable<ModuleRegistration> moduleRegistrations)
        {
            foreach (var registrationType in moduleRegistrations)
            {
                unityContainer.RegisterType(
                    typeof(NancyModule),
                    registrationType.ModuleType,
                    registrationType.ModuleKey);
            }
        }

        /// <summary>
        /// Register the default implementations of internally used types into the container as singletons
        /// </summary>
        protected sealed override void RegisterTypes(IUnityContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            container.RegisterInstance<INancyModuleCatalog>(this);

            foreach (var typeRegistration in typeRegistrations)
            {
                container.RegisterType(
                    typeRegistration.RegistrationType,
                    typeRegistration.ImplementationType,
                    new ContainerControlledLifetimeManager());
            }

            container.RegisterType(typeof(IEnumerable<IViewSourceProvider>), typeof(UnityEnumerableShim<IViewSourceProvider>));
            container.RegisterType(typeof(IEnumerable<IViewEngine>), typeof(UnityEnumerableShim<IViewEngine>));
            container.RegisterType(typeof(IEnumerable<IModelBinder>), typeof(UnityEnumerableShim<IModelBinder>));
            container.RegisterType(typeof(IEnumerable<ITypeConverter>), typeof(UnityEnumerableShim<ITypeConverter>));
            container.RegisterType(typeof(IEnumerable<IBodyDeserializer>), typeof(UnityEnumerableShim<IBodyDeserializer>));
        }

        /// <summary>
        ///   Configure the container with per-request registrations
        /// </summary>
        /// <param name = "container"></param>
        public virtual void ConfigureRequestContainer(IUnityContainer container)
        {
        }

        /// <summary>
        ///   Get all NancyModule implementation instances - should be multi-instance
        /// </summary>
        /// <returns>IEnumerable of NancyModule</returns>
        public virtual IEnumerable<NancyModule> GetAllModules(NancyContext context)
        {
            var child = unityContainer.CreateChildContainer();
            ConfigureRequestContainer(child);
            return child.ResolveAll<NancyModule>();
        }

        /// <summary>
        ///   Retrieves a specific NancyModule implementation based on its key - should be multi-instance and per-request
        /// </summary>
        /// <param name = "moduleKey">Module key</param>
        /// <returns>NancyModule instance</returns>
        public virtual NancyModule GetModuleByKey(string moduleKey, NancyContext context)
        {
            // TODO - add child container to context so it's disposed?
            var child = unityContainer.CreateChildContainer();
            ConfigureRequestContainer(child);
            return child.Resolve<NancyModule>(moduleKey);
        }
    }
}