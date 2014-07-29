﻿using System;
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
using Signum.Entities.Alerts;
using Signum.Test;
using Signum.Test.Environment;

namespace Music.Test
{
    public static class MusicExtensionsLoader
    {
        public static void Load()
        {
            using (AuthLogic.Disable())
            {
                RoleDN anonymousUserRole = null;
                RoleDN superUserRole = null;
                RoleDN internalUserRole = null;
                RoleDN externalUserRole = null;
                using (OperationLogic.AllowSave<RoleDN>())
                {
                    anonymousUserRole = new RoleDN { Name = "Anonymous", MergeStrategy = MergeStrategy.Intersection }.Save();
                    superUserRole = new RoleDN { Name = "SuperUser", MergeStrategy = MergeStrategy.Intersection }.Save();
                    internalUserRole = new RoleDN { Name = "InternalUser", MergeStrategy = MergeStrategy.Intersection }.Save();
                    externalUserRole = new RoleDN { Name = "ExternalUser", MergeStrategy = MergeStrategy.Intersection }.Save();
                }

                using (OperationLogic.AllowSave<UserDN>())
                {
                    new UserDN
                    {
                        State = UserState.Saved,
                        UserName = AuthLogic.SystemUserName,
                        PasswordHash = Security.EncodePassword(Guid.NewGuid().ToString()),
                        Role = superUserRole
                    }.Save();

                    new UserDN
                    {
                        State = UserState.Saved,
                        UserName = AuthLogic.AnonymousUserName,
                        PasswordHash = Security.EncodePassword(Guid.NewGuid().ToString()),
                        Role = anonymousUserRole
                    }.Save();

                    new UserDN
                    {
                        State = UserState.Saved,
                        UserName = "su",
                        PasswordHash = Security.EncodePassword("su"),
                        Role = superUserRole
                    }.Save();

                    new UserDN
                    {
                        State = UserState.Saved,
                        UserName = "internal",
                        PasswordHash = Security.EncodePassword("internal"),
                        Role = internalUserRole
                    }.Save();

                    new UserDN
                    {
                        State = UserState.Saved,
                        UserName = "external",
                        PasswordHash = Security.EncodePassword("external"),
                        Role = externalUserRole
                    }.Save();
                }

                Schema.Current.Initialize();

                using (AuthLogic.UnsafeUserSession("su"))
                {
                    MusicLoader.Load();

                    new AlertTypeDN { Name = "test alert" }.Execute(AlertTypeOperation.Save);
                }


                TypeConditionUsersRoles(externalUserRole.ToLite());

                TypeAuthLogic.Manual.SetAllowed(externalUserRole.ToLite(), typeof(LabelDN),
                    new TypeAllowedAndConditions(TypeAllowed.None,
                            new TypeConditionRule(MusicGroups.JapanEntities, TypeAllowed.Create)));

                TypeAuthLogic.Manual.SetAllowed(externalUserRole.ToLite(), typeof(AlbumDN),
                    new TypeAllowedAndConditions(TypeAllowed.None,
                            new TypeConditionRule(MusicGroups.JapanEntities, TypeAllowed.Create)));

                TypeConditionUsersRoles(internalUserRole.ToLite());

                ChartScriptLogic.ImportAllScripts(@"d:\Signum\Music\Extensions\Signum.Engine.Extensions\Chart\ChartScripts");
            }
        }

        private static void TypeConditionUsersRoles(Lite<RoleDN> role)
        {
            TypeAuthLogic.Manual.SetAllowed(role, typeof(UserQueryDN),
                new TypeAllowedAndConditions(TypeAllowed.None,
                        new TypeConditionRule(MusicGroups.RoleEntities, TypeAllowed.Read),
                        new TypeConditionRule(MusicGroups.UserEntities, TypeAllowed.Create)));

            TypeAuthLogic.Manual.SetAllowed(role, typeof(DashboardDN),
                new TypeAllowedAndConditions(TypeAllowed.None,
                        new TypeConditionRule(MusicGroups.RoleEntities, TypeAllowed.Read),
                        new TypeConditionRule(MusicGroups.UserEntities, TypeAllowed.Create)));

            TypeAuthLogic.Manual.SetAllowed(role, typeof(UserChartDN),
                new TypeAllowedAndConditions(TypeAllowed.None,
                        new TypeConditionRule(MusicGroups.RoleEntities, TypeAllowed.Read),
                        new TypeConditionRule(MusicGroups.UserEntities, TypeAllowed.Create)));

            TypeAuthLogic.Manual.SetAllowed(role, typeof(LinkListPartDN),
              new TypeAllowedAndConditions(TypeAllowed.None,
                      new TypeConditionRule(MusicGroups.RoleEntities, TypeAllowed.Read),
                      new TypeConditionRule(MusicGroups.UserEntities, TypeAllowed.Create)));
        }
    }
}
