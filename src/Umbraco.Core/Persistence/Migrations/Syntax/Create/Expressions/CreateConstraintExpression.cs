﻿using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateConstraintExpression : MigrationExpressionBase
    {
        public CreateConstraintExpression(ConstraintType type)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            var constraintType = (Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            if (Constraint.IsPrimaryKeyConstraint && SyntaxConfig.SqlSyntaxProvider.SupportsClustered())
                constraintType += " CLUSTERED";

            if (Constraint.IsNonUniqueConstraint)
                constraintType = string.Empty;

            var columns = new string[Constraint.Columns.Count];

            for (int i = 0; i < Constraint.Columns.Count; i++)
            {
                columns[i] = SyntaxConfig.SqlSyntaxProvider.GetQuotedColumnName(Constraint.Columns.ElementAt(i));
            }

            return string.Format(SyntaxConfig.SqlSyntaxProvider.CreateConstraint,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedName(Constraint.ConstraintName),
                                 constraintType,
                                 string.Join(", ", columns));
        }
    }
}