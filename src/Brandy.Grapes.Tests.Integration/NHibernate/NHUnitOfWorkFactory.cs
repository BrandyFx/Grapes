namespace Brandy.Trees.Tests.Integration.NHibernate
{
    using Grapes.NHibernate;
    using Grapes.Tests;
    using Infrastructure;
    using global::FluentNHibernate.Cfg;
    using global::FluentNHibernate.Cfg.Db;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::NHibernate.Mapping.ByCode;
    using global::NHibernate.Tool.hbm2ddl;

    public class NHUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private static readonly ISessionFactory sessionFactory;

        static NHUnitOfWorkFactory()
        {
            var mapper = new ModelMapper();
            mapper.Class<TestTreeEntry>(c =>
                {
                    c.Id(e => e.Id, m => m.Generator(Generators.Identity));
                    c.Property(e => e.Name, m => m.Column(col => col.Default("'The name'")));
                    c.MapTree("TestTreeClass_HIERARCHY");
                    c.DynamicInsert(true);
                    c.DynamicUpdate(true);
                });

            var config = MsSqlCeConfiguration.Standard
                .ConnectionString("Data Source=TestDb.sdf")
                .ShowSql();

            var cfg = Fluently.Configure()
                .Database(config)
                .ExposeConfiguration(ExtendConfiguration);

            var configuration = cfg.BuildConfiguration();
            configuration.AddDeserializedMapping(mapper.CompileMappingForAllExplicitlyAddedEntities(), "Test");
            BuildSchema(configuration);

            sessionFactory = configuration.BuildSessionFactory();
        }

        public IUnitOfWork Create()
        {
            return new NHUnitOfWork(sessionFactory.OpenSession());
        }

        private static void BuildSchema(Configuration configuration)
        {
            new SchemaExport(configuration).Execute(true, true, false);
        }

        private static void ExtendConfiguration(Configuration c)
        {
            c.SetProperty("generate_statistics", "true");
        }
    }
}