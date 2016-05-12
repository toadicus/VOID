// VOID_ScriptedPanels
//
// ScriptScanner.cs
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
	class ScriptScanner
	{
		internal ScriptScanner()
		{

		}

		public ScriptScanner(string input)
		{
			inputString = input;
			inputLength = inputString.Length;
			pointer = 0;
			state = StateType.NonEval;
		}

		public Token GetNextToken()
		{
			char c;
			double numericTempValue;

			StringBuilder currentToken;

			currentToken = new StringBuilder();

			while (pointer < inputLength)
			{
				c = inputString[pointer];

				switch (state)
				{
					case StateType.AfterDataVar:
						switch (c)
						{
							case tokenStopChar:
								pointer++;
								this.state = StateType.NonEval;
								return new Token(Token.TokenType.EndOfEval, c);
						}

						throw new VOIDScriptSyntaxException(string.Format(
							"Expected EndOfEval '}}' after DataVar at position {0}, found '{1}'.",
							pointer,
							c
						));
					case StateType.NonEval:
						switch (c)
						{
							// If data variables start with '$' and occur outside eval blocks.
							case tokenDataChar:
								if (pointer > 0)
								{
									throw new VOIDScriptSyntaxException("DataVars must be the only input for a given line.");
								}
								state = StateType.InDataVar;
								pointer++;
								break;
							case tokenStartChar:
								// Advance the pointer past this token.
								if (currentToken.Length == 0)
								{
									// there was no string data before this '['.

									// Change the state.
									state = StateType.InEval;

									// Advance the pointer past this '['.
									pointer++;

									return new Token(Token.TokenType.StartOfEval, null);
								}
								else
								{
									// Return the string token that occurred right before this '['.
									return new Token(
										Token.TokenType.String,
										currentToken.ToString()
									);
								}
							default:
								// Append this character to the current string token.  Consume the character.
								currentToken.Append(c);
								pointer++;
								break;
						}
						break;
					case StateType.InEval:
						switch (c)
						{
							case tokenFormatChar:
								state = StateType.InFormatString;
								pointer++;
								return new Token(Token.TokenType.FormatOperator, null);
							case tokenStopChar:
								state = StateType.NonEval;
								pointer++;
								return new Token(Token.TokenType.EndOfEval, null);
							case tokenDataChar:
								state = StateType.InDataVar;
								pointer++;
								break;
							case tokenValueChar:
								state = StateType.InValueVar;
								pointer++;
								break;
							case '+':
							case '-':
								pointer++;
								return new Token(Token.TokenType.AddOperator, c);
							case '*':
							case '/':
								//state = StateType.InEval;
								pointer++;
								return new Token(Token.TokenType.MultOperator, c);
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
							case '.':
								state = StateType.InNumValue;
								break;
							default:
								if (char.IsWhiteSpace(c))
								{
									// Skip whitespace.
									pointer++;
								}
								else
								{
									throw new VOIDScriptSyntaxException(string.Format(
										"Unexpected character '{0}' in evaluation block at position {1}." +
										"  Expected any digit, '.', '+', '-', '*', or '/'.",
										c, pointer
									));
								}
								break;
						}
						break;
					case StateType.InDataVar:
					case StateType.InValueVar:
						if (char.IsLetterOrDigit(c))
						{
							// Valid for the variable name.  Consume this character.
							currentToken.Append(c);
							pointer++;
						}
						else
						{
							// End of the token. Don't consume this character.
							if (state == StateType.InDataVar)
							{
								state = StateType.AfterDataVar;
								return new Token(Token.TokenType.DataVar, currentToken.ToString());
							}
							else
							{
								state = StateType.InEval;
								return new Token(Token.TokenType.ValueVar, currentToken.ToString());
							}
						}
						break;
					case StateType.InNumValue:
						if (char.IsDigit(c) || c == '.')
						{
							// Consume the character.
							currentToken.Append(c);
							pointer++;
						}
						else
						{
							// End of the number.  Don't consume this token.
							state = StateType.InEval;
							if (double.TryParse(currentToken.ToString(), out numericTempValue))
							{
								return new Token(Token.TokenType.NumValue, numericTempValue);
							}
							else
							{
								return new Token(Token.TokenType.NumValue, double.NaN);
							}
						}
						break;
					case StateType.InFormatString:
						switch (c)
						{
							case tokenStopChar:
								state = StateType.InEval;
								// TODO: Should probably be checking to make sure the string is not zero-length.
								return new Token(Token.TokenType.FormatString, currentToken.ToString());
							case tokenFormatParameterChar:
								state = StateType.InFormatParam;
								break;
							default:
								currentToken.Append(c);
								pointer++;
								break;
						}
						break;
					case StateType.InFormatParam:
						switch (c)
						{
							case tokenStopChar:
								state = StateType.InEval;

								if (currentToken.Length > 0)
								{
									int defaultParameter;

									if (Int32.TryParse(currentToken.ToString(), out defaultParameter))
									{
										return new Token(Token.TokenType.FormatParameter, defaultParameter);
									}
								}

								return new Token(Token.TokenType.FormatParameter, currentToken.ToString());
							case tokenFormatParameterChar:
								pointer++;
								break;
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
								currentToken.Append(c);
								pointer++;
								break;
							default:
								throw new VOIDScriptSyntaxException(string.Format(
										"Unexpected character '{0}' in format parameter at position '{1}':\n\t" +
										"Expected '{2}' or end of evaluation block.",
										c,
										pointer,
										tokenFormatParameterChar
									));
						}
						break;
					default:
						throw new NotImplementedException("Unexpected StateType.  This should be impossible; please report!");
				}
			}

			// Fall through: if we didn't have any evaluation blocks, just return the whole string.
			if (currentToken.Length == 0)
			{
				// We've reached the end of the line.
				return new Token(Token.TokenType.EndOfLine, null);
			}
			else
			{
				return new Token(Token.TokenType.String, currentToken.ToString());
			}
		}

		public StateType state
		{
			get;
			private set;
		}

		private string inputString;
		private int inputLength;
		private int pointer;

		private const char tokenStartChar = '[';
		private const char tokenStopChar = ']';
		private const char tokenDataChar = '$';
		private const char tokenValueChar = '#';
		private const char tokenFormatChar = ':';
		private const char tokenFormatParameterChar = '%';

		internal enum StateType
		{
			NonEval,
			InEval,
			InDataVar,
			InValueVar,
			InNumValue,
			AfterDataVar,
			InFormatString,
			InFormatParam
		}
	}
}

