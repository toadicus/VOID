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
using VOID;

namespace VOID_ScriptedPanels
{
	class ScriptParser
	{
		private ScriptScanner scanner;

		private List<Token> Tokens;

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

		public ScriptParser(string input)
		{
			this.scanner = new ScriptScanner(input);
			this.Tokens = new List<Token>();
		}

		public LambdaExpression Parse()
		{
			this.CurrentToken = this.scanner.GetNextToken();

			return (LambdaExpression)this.ParseLambda();
		}

		private Expression ParseLambda()
		{
			Expression expr = ParseString();

			return Expression.Lambda(expr);
		}

		private Expression ParseString()
		{
			Expression left = ParseAdditive();

			while (
				this.CurrentToken.Type == Token.TokenType.String ||
				this.CurrentToken.Type == Token.TokenType.StartOfEval ||
				this.CurrentToken.Type == Token.TokenType.EndOfEval
			)
			{
				this.CurrentToken = this.scanner.GetNextToken();

				Expression right = this.ParseAdditive();

				right = Expression.Call(right, "ToString", new Type[] {});

				left = Expression.Call(
					null,
					typeof(String).GetMethod("Concat", new Type[] { typeof(String), typeof(String) }
					),
					left, right
				);
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
			MemberInfo[] members;
			MemberInfo member = null;

			switch (this.CurrentToken.Type)
			{
				case Token.TokenType.NumValue:
				case Token.TokenType.String:
					return Expression.Constant(this.CurrentToken.Value);
				case Token.TokenType.EndOfLine:
					return Expression.Constant(string.Empty);
					// TODO: Change DataVar to do whole guihorizontal.
				case Token.TokenType.DataVar:
				case Token.TokenType.ValueVar:
					members = typeof(VOID_Data).GetMember((string)this.CurrentToken.Value);

					if (members != null && members.Length > 0)
					{
						member = members[0];
					}

					if (member is System.Reflection.FieldInfo)
					{
						return Expression.Field(null, member as FieldInfo);
					}
					else if (member is System.Reflection.PropertyInfo)
					{
						return Expression.Property(null, member as PropertyInfo);
					}
					else
					{
						throw new Exception(string.Format(
							"VOID_Data does not contain a field or property named '{0}'",
							this.CurrentToken.Value
						));
					}
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
	}
}

