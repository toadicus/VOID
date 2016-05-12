// VOID_ScriptedPanels
//
// ScriptParser.cs
//
// Copyright © 2015, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ToadicusTools;
using VOID;

namespace VOID_ScriptedPanels
{
	class ScriptParser
	{
		private ScriptScanner scanner;

		private List<Token> Tokens;

		private Expression formatter;
		private MethodInfo toSIMethod;

		private List<ParameterExpression> lambdaParameters;
		private List<ParameterSig> parameterSignatures;

		private Token CurrentToken
		{
			get {
				return this.Tokens[this.Tokens.Count - 1];
			}
			set {
				this.Tokens.Add(value);
			}
		}

		private Token PreviousToken
		{
			get {
				return this.Tokens[this.Tokens.Count - 2];
			}
		}

		public IList<ParameterSig> ParameterSignatures
		{
			get;
			private set;
		}

		public ScriptParser(string input)
		{
			this.scanner = new ScriptScanner(input);
			this.Tokens = new List<Token>();
			this.lambdaParameters = new List<ParameterExpression>();
			this.formatter = Expression.Constant(ToadicusTools.Text.SIFormatProvider.SIFormatter);
			this.toSIMethod = typeof(ToadicusTools.Text.SIFormatProvider).GetMethod(
				"ToSI",
				new Type[]
				{
					typeof(double),
					typeof(int)
				}
			);
			this.parameterSignatures = new List<ParameterSig>();
			this.ParameterSignatures = this.parameterSignatures.AsReadOnly();
		}

		public LambdaExpression Parse()
		{
			this.CurrentToken = this.scanner.GetNextToken();

			return (LambdaExpression)this.ParseLambda();
		}

		private Expression ParseLambda()
		{
			Expression expr = ParseString();

			return Expression.Lambda(expr, this.lambdaParameters.ToArray());
		}

		private Expression ParseString()
		{
			Expression left = ParseFormat();

			while (
				this.CurrentToken.Type == Token.TokenType.String ||
				this.CurrentToken.Type == Token.TokenType.StartOfEval ||
				this.CurrentToken.Type == Token.TokenType.EndOfEval
			)
			{
				this.CurrentToken = this.scanner.GetNextToken();

				Expression right = this.ParseFormat();

				if (left.Type != typeof(string))
				{
					left = Expression.Call(left, "ToString", new Type[] { });
				}

				if (right.Type != typeof(string))
				{
					right = Expression.Call(right, "ToString", new Type[] { });
				}

				left = Expression.Call(
					null,
					typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) }),
					left, right
				);
			}

			return left;
		}

		// TODO: FIX THIS
		private Expression ParseFormat()
		{
			Expression left = this.ParseAdditive();
			Expression right;

			if (this.CurrentToken.Type == Token.TokenType.FormatOperator)
			{
				this.CurrentToken = this.scanner.GetNextToken();

				switch (this.CurrentToken.Type)
				{
					case Token.TokenType.FormatString:
						right = Expression.Constant(string.Concat("{0:", this.CurrentToken.Value, "}"));

						left = Expression.Convert(left, typeof(object));

						left = Expression.NewArrayInit(typeof(object), left);

						var formatMethod = typeof(string).GetMethod("Format", new Type[]
							{
								typeof(IFormatProvider),
								typeof(string),
								typeof(object[])
							});

						left = Expression.Call(
							null,
							formatMethod,
							formatter,
							right,
							left
						);

						break;
					case Token.TokenType.FormatParameter:
						ParameterSig param = new ParameterSig();
						param.Name = "digits";
						param.Type = typeof(int);
						if (this.CurrentToken.Value is int)
						{
							param.DefaultValue = this.CurrentToken.Value;
						}

						right = Expression.Parameter(param.Type, param.Name);
						this.lambdaParameters.Add((ParameterExpression)right);
						this.parameterSignatures.Add(param);

						left = Expression.Convert(left, typeof(double));

						left = Expression.Call(null, this.toSIMethod, left, right);

						break;
					default:
						throw new VOIDScriptParserException(string.Format(
								"Found unexpected {0} '{1}' after FormatOperator.  Format string or '%' must follow FormatOperator.",
								Enum.GetName(typeof(Token.TokenType), this.CurrentToken.Type),
								this.CurrentToken.Value
							));
				}

				this.CurrentToken = this.scanner.GetNextToken();
			}

			return left;
		}

		private Expression ParseAdditive()
		{
			Expression left = this.ParseMultiplicative();

			while (this.CurrentToken.Type == Token.TokenType.AddOperator)
			{
				Token op = this.CurrentToken;

				this.CurrentToken = this.scanner.GetNextToken();

				Expression right = this.ParseMultiplicative();

				if ((char)op.Value == '+')
				{
					left = Expression.Add(left, right);
				}
				if ((char)op.Value == '-')
				{
					left = Expression.Subtract(left, right);
				}
			}

			return left;
		}

		private Expression ParseMultiplicative()
		{
			Expression left = this.ParseConstant();

			while (this.CurrentToken.Type == Token.TokenType.MultOperator)
			{
				Token op = this.CurrentToken;

				this.CurrentToken = this.scanner.GetNextToken();

				Expression right = this.ParseConstant();

				if ((char)op.Value == '*')
				{
					left = Expression.Multiply(left, right);
				}
				if ((char)op.Value == '/')
				{
					left = Expression.Divide(left, right);
				}
			}

			return left;
		}

		private Expression ParseConstant()
		{
			Expression constant = this.FindConstant();

			if (constant != null)
			{
				this.CurrentToken = this.scanner.GetNextToken();
			}

			return constant;
		}

		private Expression FindConstant()
		{
			switch (this.CurrentToken.Type)
			{
				case Token.TokenType.NumValue:
				case Token.TokenType.String:
					return Expression.Constant(this.CurrentToken.Value);
				case Token.TokenType.EndOfLine:
					return Expression.Constant(string.Empty);
				case Token.TokenType.DataVar:
				case Token.TokenType.ValueVar:
					string varName = (string)this.CurrentToken.Value;
					MemberInfo[] members = typeof(VOID_Data).GetMember(varName);
					MemberInfo member = null;

					if (members.Length > 0)
					{
						member = members[0];
					}

					if (
						member == null ||
						(!(member is System.Reflection.FieldInfo) && !(member is System.Reflection.PropertyInfo)))
					{
						// TODO: Implement access through VOID_Data.DataValue.
						throw new Exception(string.Format(
								"VOID_Data does not contain a field or property named '{0}'",
								this.CurrentToken.Value
							));
					}

					MemberExpression data;

					if (member is System.Reflection.FieldInfo)
					{
						FieldInfo field = member as FieldInfo;

						data = Expression.Field(null, field);

						var fieldType = field.FieldType;

						if (typeof(IVOID_DataValue).IsAssignableFrom(fieldType))
						{
							data = Expression.Property(Expression.Convert(data, typeof(IVOID_DataValue)), "Value");
						}
					}
					else
					{
						PropertyInfo property = member as PropertyInfo;

						data = Expression.Property(null, property);

						var propType = property.PropertyType;

						if (typeof(IVOID_DataValue).IsAssignableFrom(propType))
						{
							data = Expression.Property(Expression.Convert(data, typeof(IVOID_DataValue)), "Value");
						}
					}

					return data;
				case Token.TokenType.StartOfEval:
				case Token.TokenType.EndOfEval:
					this.CurrentToken = this.scanner.GetNextToken();
					return this.FindConstant();
				default:
					throw new FormatException(string.Format(
						"Expected number literal or VOID_Data field, got {0} '{1}' at element {2}",
						Enum.GetName(typeof(Token.TokenType), this.CurrentToken.Type),
						this.CurrentToken.Value,
						this.Tokens.Count
					));
			}
		}

		public struct ParameterSig
		{
			public string Name;
			public Type Type;
			public object DefaultValue;
		}
	}
}

