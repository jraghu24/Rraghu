﻿using System.Data;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsPropertyTypeGroupTable : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("cmsPropertyTypeGroup").AddColumn("parentGroupId").AsInt16().Nullable();

            Create.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup")
                .FromTable("cmsPropertyTypeGroup").ForeignColumn("parentGroupId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup").OnTable("cmsPropertyTypeGroup");

            Delete.Column("parentGroupId").FromTable("cmsPropertyTypeGroup");
        }
    }
}