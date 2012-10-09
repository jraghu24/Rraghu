﻿using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoRelation")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("parentId")]
        public int ParentId { get; set; }

        [Column("childId")]
        public int ChildId { get; set; }

        [Column("relType")]
        public int RelationType { get; set; }

        [Column("datetime")]
        public DateTime Datetime { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}