using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Engine.DynamicQuery;
using Signum.Engine.Authorization;
using Signum.Engine.Operations;
using Signum.Engine.Maps;
using Signum.Engine;
using Signum.Entities.Authorization;
using Signum.Entities;
using Signum.Services;
using Signum.Engine.Basics;
using Signum.Engine.Excel;
using Signum.Engine.Dashboard;
using Signum.Entities.Dashboard;
using Signum.Entities.Excel;
using Signum.Entities.Chart;
using Signum.Engine.UserQueries;
using Signum.Entities.UserQueries;
using Signum.Entities.Basics;
using Signum.Engine.Chart;
using Signum.Engine.Cache;
using Signum.Engine.Files;
using Signum.Engine.Processes;
using Signum.Entities.Processes;
using Signum.Engine.Alerts;
using Signum.Engine.Notes;
using Signum.Test;
using Signum.Utilities;
using Signum.Test.Environment;
using Signum.Engine.Translation;
using System.Threading;
using System.Globalization;
using Signum.Entities.Alerts;
using Signum.Entities.Notes;
using Signum.Entities.DiffLog;
using Signum.Engine.DiffLog;

namespace Music.Test
{
    public static class Starter
    {
        static bool hasData = false;
        public static void StartAndLoad(string connectionString)
        {
            Start(connectionString);

            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

            if (!hasData)
            {
                Administrator.TotalGeneration();

                using (AuthLogic.Disable())
                {
                    Schema.Current.Initialize();

                    MusicExtensionsLoader.Load();
                }

                hasData = true;
            }
        }

        public static void Dirty()
        {
            hasData = false;
        }

        static bool started = false;
        public static void Start(string connectionString)
        {
            if (!started)
            {
                SchemaBuilder sb = new SchemaBuilder();
                DynamicQueryManager dqm = new DynamicQueryManager();
                Connector.Default = new SqlConnector(connectionString, sb.Schema, dqm, SqlServerVersion.SqlServer2008);
                sb.Schema.Version = typeof(Starter).Assembly.GetName().Version;
                sb.Schema.ForceCultureInfo = CultureInfo.GetCultureInfo("en-GB");

                MixinDeclarations.Register<OperationLogEntity, DiffLogMixin>();
                MixinDeclarations.Register<ProcessEntity, UserProcessSessionMixin>();
                OverrideImplementations(sb);

                CacheLogic.Start(sb);
                TypeLogic.Start(sb, dqm);

                OperationLogic.Start(sb, dqm);
                DiffLogLogic.Start(sb, dqm);

                CultureInfoLogic.Start(sb, dqm); 
                ExceptionLogic.Start(sb, dqm);
                AuthLogic.Start(sb, dqm, "System", "Anonymous");
                UserTicketLogic.Start(sb, dqm);

                ProcessLogic.Start(sb, dqm, userProcessSession: true);
                PackageLogic.Start(sb, dqm, true, true);          

                AuthLogic.StartAllModules(sb, dqm);

                QueryLogic.Start(sb);
                UserQueryLogic.Start(sb, dqm);
                UserQueryLogic.RegisterUserTypeCondition(sb, MusicGroups.UserEntities);
                UserQueryLogic.RegisterRoleTypeCondition(sb, MusicGroups.RoleEntities);

                ChartLogic.Start(sb, dqm);
                UserChartLogic.RegisterUserTypeCondition(sb, MusicGroups.UserEntities);
                UserChartLogic.RegisterRoleTypeCondition(sb, MusicGroups.RoleEntities);

                DashboardLogic.Start(sb, dqm);
                DashboardLogic.RegisterUserTypeCondition(sb, MusicGroups.UserEntities);
                DashboardLogic.RegisterRoleTypeCondition(sb, MusicGroups.RoleEntities);

                AlertLogic.Start(sb, dqm, new [] { typeof(LabelEntity) });
                NoteLogic.Start(sb, dqm, new[] { typeof(LabelEntity) });

                FilePathLogic.Start(sb, dqm);
                ExcelLogic.Start(sb, dqm, true);

                MusicLogic.Start(sb, dqm);

                TypeConditionLogic.Register<LabelEntity>(MusicGroups.JapanEntities, l => l.Country.Name.StartsWith(MusicLoader.Japan) || l.Owner != null && l.Owner.Entity.Country.Name.StartsWith(MusicLoader.Japan));
                TypeConditionLogic.Register<AlbumEntity>(MusicGroups.JapanEntities, a => a.Label.InCondition(MusicGroups.JapanEntities));

                CacheLogic.CacheTable<LabelEntity>(sb);


                started = true;

                sb.ExecuteWhenIncluded();
            }
        }

        private static void OverrideImplementations(SchemaBuilder sb)
        {
            sb.Settings.FieldAttributes((DashboardEntity cp) => cp.Owner).Replace(new ImplementedByAttribute(typeof(UserEntity), typeof(RoleEntity)));
            sb.Settings.FieldAttributes((UserQueryEntity uq) => uq.Owner).Replace(new ImplementedByAttribute(typeof(UserEntity), typeof(RoleEntity)));
            sb.Settings.FieldAttributes((UserChartEntity uq) => uq.Owner).Replace(new ImplementedByAttribute(typeof(UserEntity), typeof(RoleEntity)));

            sb.Schema.Settings.FieldAttributes((ProcessEntity cp) => cp.Data).Replace(new ImplementedByAttribute(typeof(PackageEntity), typeof(PackageOperationEntity)));
            sb.Schema.Settings.FieldAttributes((PackageLineEntity cp) => cp.Package).Replace(new ImplementedByAttribute(typeof(PackageEntity), typeof(PackageOperationEntity)));
            sb.Schema.Settings.FieldAttributes((ProcessExceptionLineEntity cp) => cp.Line).Replace(new ImplementedByAttribute(typeof(PackageLineEntity)));
            sb.Schema.Settings.FieldAttributes((ProcessEntity cp) => cp.Mixin<UserProcessSessionMixin>().User).Replace(new ImplementedByAttribute(typeof(UserEntity)));

            sb.Schema.Settings.FieldAttributes((OperationLogEntity ol) => ol.User).Replace(new ImplementedByAttribute(typeof(UserEntity)));
            sb.Schema.Settings.FieldAttributes((ExceptionEntity e) => e.User).Replace(new ImplementedByAttribute(typeof(UserEntity)));

            sb.Schema.Settings.FieldAttributes((AlertEntity e) => e.CreatedBy).Replace(new ImplementedByAttribute(typeof(UserEntity)));
            sb.Schema.Settings.FieldAttributes((AlertEntity e) => e.AttendedBy).Replace(new ImplementedByAttribute(typeof(UserEntity)));
            sb.Schema.Settings.FieldAttributes((NoteEntity e) => e.CreatedBy).Replace(new ImplementedByAttribute(typeof(UserEntity)));
        }

    }

    public static class MusicGroups
    {
        public static readonly TypeConditionSymbol JapanEntities = new TypeConditionSymbol();
        public static readonly TypeConditionSymbol RoleEntities = new TypeConditionSymbol();
        public static readonly TypeConditionSymbol UserEntities = new TypeConditionSymbol();
    }
}
