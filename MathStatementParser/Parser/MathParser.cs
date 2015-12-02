﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathStatementParser;
using MathStatementParser.Lexer;

namespace MathStatementParser.Parser
{
    /// <summary>
    /// 数式の構文解析器
    /// 構文解析結果として抽象構文木を出力する。
    /// </summary>
    public class MathParser : Parser
    {
        #region SYNTAX
        /* ******************************************
         * <expr>    ::= <term>   (('+'|'-') <term>   )*
         * <term>    ::= <factor> (('*'|'/') <factor> )*
         * <factor>  ::= '(' <expr> ')' | <element>
         * <elements>::= Integer | real-number
         * ***************************************** */
        #endregion

        #region FIELDS
        #endregion

        #region PROPERTY
        #endregion

        #region METHODS        
        /// <summary>
        /// Initializes a new instance of the <see cref="MathParser"/> class.
        /// </summary>
        /// <param name="lex">The lexer class's instance</param>
        public MathParser(Lexer.Lexer lex)
            :base(lex)
        {
        }

        /// <summary>
        /// Tests parsing if it's ok or ng.
        /// </summary>
        /// <returns>
        /// null : success , not null : error
        /// </returns>
        public override string Test()
        {
            try
            {
                var tree = Expr();
                // Accepted 
                if( LOOK_AHEAD.Type == MathLexer.TYPE_EOF )
                {
                    return null;
                } else {
                    return string.Format("Syntax Error : TYPE:{0},TOKEN:{1}",LOOK_AHEAD.Type,LOOK_AHEAD.Text); 
                }
                
            }catch(ParserException ex)
            {
                return "Syntax Error" + ex.Message;
            }
        }
        /// <summary>
        /// Parses input to a Abstract Syntax Tree.
        /// 入力字句を抽象構文木へ変換する
        /// </summary>
        /// <returns>
        /// 生成された抽象構文木。失敗時はnull
        /// </returns>
        public override Tree.AbstractSyntaxTree ParseAst()
        {
            try
            {
                var tree = Expr();
                // Accepted 
                if (LOOK_AHEAD.Type == MathLexer.TYPE_EOF)
                {
                    return tree;
                }
                else
                {
                    return null;
                }

            }
            catch (ParserException)
            {
                return null;
            }

        }
        #endregion

        #region SYNTAX_PARSE_IMPLEMENTS
        /// <summary>
        /// 文法 : expr を表現
        /// <![CDATA[ <expr>    ::= <term> (('+'|'-') <term>)* ]]>
        /// </summary>
        /// <remarks>抽象構文木(Abstract Syntax Tree)の部分木 or 全体</remarks>
        Tree.AbstractSyntaxTree Expr()
        {
            var left = Term();

            while (LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_OPE_ADD
                || LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_OPE_SUB)
            {
                var op = Match(LOOK_AHEAD.Type);
                var right = Term();

                op.AddChild(left);
                op.AddChild(right);

                // 優先度が同じ場合に左側を優先。
                // 左側演算子の階層を深く構築する。
                left = op;
            }
            return left;
        }
        /// <summary>
        /// 文法：termを表現
        /// <![CDATA[<term>    ::= <factor> (('*'|'/') <factor>)* ]]>
        /// </summary>
        /// <remarks>抽象構文木(Abstract Syntax Tree)の部分木</remarks>
        Tree.AbstractSyntaxTree Term()
        {
            var left = Factor();

            while (LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_OPE_MUL
             || LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_OPE_DIV)
            {
                var op = Match(LOOK_AHEAD.Type);
                var right = Factor();

                op.AddChild(left);
                op.AddChild(right);

                // 優先度が同じ場合に左側を優先。
                // 左側演算子の階層を深く構築する。
                left = op;
            }
            return left;
        }
        /// <summary>
        ///文法：factorを表現
        ///<![CDATA[<factor>  ::= '(' <expr> ')' | <element>]]>
        /// </summary>
        /// <remarks>抽象構文木(Abstract Syntax Tree)の部分木</remarks>
        Tree.AbstractSyntaxTree Factor()
        {
            if( LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_LPAREN)
            {
                Match(Lexer.MathLexer.TYPE_LPAREN);
                var tree = Expr();
                Match(Lexer.MathLexer.TYPE_RPAREN);
                return tree;
            }
            else
            {
                return Elements();
            }
        }
        /// <summary>
        /// 文法：elementesを表現
        /// <![CDATA[<elements> ::= Integer | real-number]]>
        /// </summary>
        /// <exception cref="MathStatementParser.Parser.ParserException"></exception>
        /// <remarks>抽象構文木(Abstract Syntax Tree)の部分木</remarks>
        Tree.AbstractSyntaxTree Elements()
        {
            if(LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_NUM)
            {
                return Match(Lexer.MathLexer.TYPE_NUM);
            }
            else if( LOOK_AHEAD.Type == Lexer.MathLexer.TYPE_REAL)
            {
                return Match(Lexer.MathLexer.TYPE_REAL);
            }
            else
            {
                throw new ParserException(string.Format("Syntax Error : Invalid Token comes [{0}]",Lexer.MathLexer.tokenNames[LOOK_AHEAD.Type]));
            }
        }
        #endregion
    }
}
