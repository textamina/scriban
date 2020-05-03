// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("block statement", "<statement>...end")]
    public sealed partial class ScriptBlockStatement : ScriptStatement
    {
        private ScriptList<ScriptStatement> _statements;

        public ScriptBlockStatement()
        {
            Statements = new ScriptList<ScriptStatement>();
        }

        public ScriptList<ScriptStatement> Statements
        {
            get => _statements;
            set => ParentToThis(ref _statements, value);
        }

        public bool HasReturn => _statements != null && _statements.Count > 1 && _statements[_statements.Count - 2] is ScriptReturnStatement;

        public override object Evaluate(TemplateContext context)
        {
            object result = null;
            var statements = Statements;
            for (int i = 0; i <  statements.Count; i++)
            {
                var statement =  statements[i];

                // Throw if cancellation is requested
                if (context.CancellationToken.IsCancellationRequested)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                }

                if (statement.CanSkipEvaluation)
                {
                    continue;
                }

                result = context.Evaluate(statement);

                // Top-level assignment expression don't output anything
                if (!statement.CanOutput)
                {
                    result = null;
                }
                else if (result != null && context.FlowState != ScriptFlowState.Return && context.EnableOutput)
                {
                    context.Write(Span, result);
                    result = null;
                }

                // If flow state is different, we need to exit this loop
                if (context.FlowState != ScriptFlowState.None)
                {
                    break;
                }
            }
            return result;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            foreach (var scriptStatement in Statements)
            {
                printer.Write(scriptStatement);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }
    }
}