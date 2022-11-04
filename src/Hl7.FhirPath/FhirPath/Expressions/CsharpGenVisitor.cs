/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */
using Hl7.Fhir.Serialization;
using Hl7.FhirPath.Functions;
using System;
using System.Linq;
using System.Text;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.FhirPath.Expressions
{
    public class CSharpGenVisitor : ExpressionVisitor<string>
    {
        //        private bool ImmediateLambdaScope { get; set; }

        //private CSharpGenVisitor with(Action<CSharpGenVisitor> mods)
        //{
        //    var result = new CSharpGenVisitor { ImmediateLambdaScope = ImmediateLambdaScope };
        //    mods(result);

        //    return result;
        //}

        /*
         *   Lexer.String.Select(v => new ConstantExpression(v, TypeSpecifier.String))
                .Or(Lexer.DateTime.Select(v => new ConstantExpression(v, TypeSpecifier.DateTime)))
                .Or(Lexer.Date.Select(v => new ConstantExpression(v, TypeSpecifier.Date)))
                .Or(Lexer.Time.Select(v => new ConstantExpression(v, TypeSpecifier.Time)))
                .XOr(Lexer.Bool.Select(v => new ConstantExpression(v, TypeSpecifier.Boolean)))
                .Or(Quantity.Select(v => new ConstantExpression(v, TypeSpecifier.Quantity)))
                .Or(Lexer.DecimalNumber.Select(v => new ConstantExpression(v, TypeSpecifier.Decimal)))
                .Or(Lexer.IntegerNumber.Select(v => new ConstantExpression(v, TypeSpecifier.Integer)));
        */

        public override string VisitConstant(ConstantExpression expression)
        {
            var value = expression.Value switch
            {
                string s => toss(s),
                P.DateTime dt => $"DateTime.Parse({toss(dt.ToStringRepresentation())})",
                P.Date d => $"Date.Parse({toss(d.ToStringRepresentation())})",
                P.Time tim => $"Time.Parse({toss(tim.ToStringRepresentation())})",
                bool b => tos(b),
                P.Quantity q => $"new Quantity({tos(q.Value)}, {toss(q.Unit)}, QuantityUnitSystem.{q.System})",
                decimal d => PrimitiveTypeConverter.ConvertTo<string>(d),
                int i => i.ToString(),
                _ => throw new NotSupportedException()
            };

            return $"ElementNode.ForPrimitive({value})";

            static string tos(object o) => PrimitiveTypeConverter.ConvertTo<string>(o);
            static string toss(string s) => "\"" + s + "\"";   // TODO: quote some stuff
        }


        private static int callDepth(FunctionCallExpression fe)
        {
            int depth = 0;
            var scan = fe;

            while (scan.Focus is FunctionCallExpression fep)
            {
                scan = fep;
                depth += 1;
            }

            return depth;
        }

        public override string VisitFunctionCall(FunctionCallExpression expression)
        {

            //            var depth = callDepth(expression);
            StringBuilder b = new();

            switch (expression.FunctionName)
            {
                case "builtin.children":
                    {
                        b.Append(expression.Focus.Accept(this));
                        b.Append($".Navigate(");
                        var childName = expression.Arguments.OfType<ConstantExpression>().Single().Value;
                        b.Append($"\"{childName}\")");
                        break;
                    }
                case "where":
                    {
                        b.Append(expression.Focus.Accept(this));
                        b.Append(".Where(@this => ");
                        var nested = expression.Arguments.Single().Accept(this);
                        if (nested.StartsWith("focus."))
                            nested = "@this." + nested.Substring(6);
                        b.Append(nested);
                        b.Append(")");
                        break;
                    }
                default:
                    {
                        b.Append(expression.Focus.Accept(this));
                        b.Append(".One(focus => ");
                        b.Append($"focus.{toCallable(expression.FunctionName)}(");
                        var args = expression.Arguments.Select(a => a.Accept(this));
                        b.Append(string.Join(", ", args));
                        b.Append(")");
                        b.Append(")");
                        break;
                    }
            };

            return b.ToString();
        }

        private static string toCallable(string fname) => fname.Replace('.', '_');

        public override string VisitNewNodeListInit(NewNodeListInitExpression expression)
        {
            StringBuilder b = new();

            b.Append("new List<Base>() {");

            foreach (var element in expression.Contents)
                element.Accept(this);

            b.Append("}");
            return b.ToString();
        }

        public override string VisitVariableRef(VariableRefExpression expression)
        {
            return expression.Name switch
            {
                "builtin.that" => "focus",
                var name => name
            };
        }

    }

    public static class CsharpGenVisitorExtensions
    {
        public static string ToCsharp(this Expression expr)
        {
            var csharper = new CSharpGenVisitor();
            return "focus => " + expr.Accept(csharper);
        }
    }

}
