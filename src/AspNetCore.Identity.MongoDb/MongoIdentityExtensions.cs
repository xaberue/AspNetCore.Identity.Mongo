using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDb.Helpers;
using AspNetCore.Identity.MongoDb.Migrations;
using AspNetCore.Identity.MongoDb.Models;
using AspNetCore.Identity.MongoDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace AspNetCore.Identity.MongoDb
{
    public static class MongoIdentityExtensions
    {
        public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services)
            where TUser : MongoUser
        {
            return services.AddIdentityMongoDbProvider<TUser, MongoRole<ObjectId>, ObjectId>(x => { });
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TKey>(this IServiceCollection services)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
        {
            return services.AddIdentityMongoDbProvider<TUser, MongoRole<TKey>, TKey>(x => { });
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
            Action<MongoIdentityOptions> setupDatabaseAction)
            where TUser : MongoUser
        {
            return services.AddIdentityMongoDbProvider<TUser, MongoRole, ObjectId>(setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TKey>(this IServiceCollection services,
            Action<MongoIdentityOptions> setupDatabaseAction)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
        {
            return services.AddIdentityMongoDbProvider<TUser, MongoRole<TKey>, TKey>(setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
            where TUser : MongoUser
            where TRole : MongoRole
        {
            return services.AddIdentityMongoDbProvider<TUser, TRole, ObjectId>(setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKey>(this IServiceCollection services,
            Action<MongoIdentityOptions> setupDatabaseAction)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            return services.AddIdentityMongoDbProvider<TUser, TRole, TKey>(x => { }, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
        {
            return services.AddIdentityMongoDbProvider<MongoUser, MongoRole, ObjectId>(setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
        {
            return services.AddIdentityMongoDbProvider<TUser, MongoRole, ObjectId>(setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKey>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction, IdentityErrorDescriber identityErrorDescriber = null)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            var dbOptions = new MongoIdentityOptions();
            setupDatabaseAction(dbOptions);

            var migrationCollection = MongoUtil.FromConnectionString<MigrationHistory>(dbOptions, dbOptions.MigrationCollection);
            var migrationUserCollection = MongoUtil.FromConnectionString<MigrationMongoUser<TKey>>(dbOptions, dbOptions.UsersCollection);
            var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions, dbOptions.UsersCollection);
            var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions, dbOptions.RolesCollection);

            // apply migrations before identity services resolved
            Migrator.Apply<MigrationMongoUser<TKey>, TRole, TKey>(migrationCollection, migrationUserCollection, roleCollection);

            var builder = services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (x => { }));

            builder.AddRoleStore<RoleStore<TRole, TKey>>()
            .AddUserStore<UserStore<TUser, TRole, TKey>>()
            .AddUserManager<UserManager<TUser>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddDefaultTokenProviders();

            services.AddSingleton(x => userCollection);
            services.AddSingleton(x => roleCollection);

            // register custom ObjectId TypeConverter
            if (typeof(TKey) == typeof(ObjectId))
            {
                TypeConverterResolver.RegisterTypeConverter<ObjectId, ObjectIdConverter>();
            }

            // Identity Services
            services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole, TKey>(roleCollection, identityErrorDescriber));
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole, TKey>(userCollection, roleCollection, identityErrorDescriber));

            return builder;
        }
    }
}