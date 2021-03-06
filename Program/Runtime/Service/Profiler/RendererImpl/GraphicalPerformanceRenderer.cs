﻿// zlib/libpng License
//
// Copyright (c) 2018 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static IceMilkTea.Profiler.GLHelper;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をユーザーが視認出来るディスプレイへ、グラフィカルにレンダリングするクラスです
    /// </summary>
    public class GraphicalPerformanceRenderer : PerformanceRenderer
    {
        private const int FontSize = 20; //フォントサイズ
        private const float RowHeight = 20; //テキストやバーを表示する行の高さ
        private const float BarMaxWidthPercentage = 90; //バーの最大横幅(画面サイズに対する%)
        private const float FontMarginLeftPercentage = 2; //テキストの左側の余白
        private const float BarMarginLeftPercentage = 2;//バーの左側の余白

        private const float MaxMillisecondPerFrame = 33; //バーで計測できる1フレーム毎の実行時間(ミリ秒)
        private const int MaxValueCacheMilliseconds = 1000; //直近の最大値をどれだけの時間キャッシュするか(ミリ秒)
        private static Color UpdateColor = Color.blue;
        private static Color LateUpdateColor = new Color(0, 1, 1);
        private static Color FixedUpdateColor = new Color(0.5f, 0, 1);
        private static Color RenderingColor = Color.green;
        private static Color TextureRenderingColor = Color.yellow;

        // メンバ変数宣言
        private UnityStandardLoopProfileResult result;
        private Font builtinFont;
        private Material barMaterial;
        private Vector2 screenSize;
        private float fontMarginLeft;
        private float barMarginLeft;
        private float barMaxWidth;
        private float textScale;

        private double lowPassFixedValue;
        private double lowPassUpdateValue;
        private double lowPassLateUpdateValue;
        private double lowPassRenderingValue;
        private double lowPassRenderTextureRenderingValue;
        private CharacterHelper characterHelper;

        /// <summary>
        /// GraphicalPerformanceRendererのコンストラクタです。
        /// </summary>
        public GraphicalPerformanceRenderer()
        {
            this.builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            this.barMaterial = new Material(Shader.Find("GUI/Text Shader"));
            this.screenSize = new Vector2(Screen.width, Screen.height);

            this.fontMarginLeft = FontMarginLeftPercentage / 100 * screenSize.x;
            this.barMarginLeft = BarMarginLeftPercentage / 100 * screenSize.x;
            this.barMaxWidth = BarMaxWidthPercentage / 100 * screenSize.x;
            this.textScale = RowHeight / FontSize;

            this.characterHelper = GLHelper.CreateCharacterHelper(this.builtinFont, FontSize, this.screenSize);

            this.FontCharacterInitialize();
        }
        /// <summary>
        /// 描画に使用するFontの初期化を行います
        /// </summary>
        private void FontCharacterInitialize()
        {
            this.builtinFont.RequestCharactersInTexture("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-&_=:;%()[]?{}|/.,", FontSize);
        }

        /// <summary>
        /// 出力の準備を行います
        /// </summary>
        /// <param name="profileFetchResults">パフォーマンスモニタから渡されるすべての計測結果の配列</param>
        public override void Begin(ProfileFetchResult[] profileFetchResults)
        {
            if (this.result == null)
            {
                // プロファイル結果を覚える
                this.result = profileFetchResults.First(x => x is UnityStandardLoopProfileResult) as UnityStandardLoopProfileResult;
            }
        }


        /// <summary>
        /// 出力を終了します
        /// </summary>
        public override void End()
        {

        }



        /// <summary>
        /// プロファイル結果をレンダリングします
        /// </summary>
        public override void Render()
        {
            // やや強めにローパスフィルタをかける（傾きの係数は後で外から渡せるように改良する）
            var lowPassFactor = 0.3;
            lowPassFixedValue = (result.FixedUpdateTime - lowPassFixedValue) * lowPassFactor + lowPassFixedValue;
            lowPassUpdateValue = (result.UpdateTime - lowPassUpdateValue) * lowPassFactor + lowPassUpdateValue;
            lowPassLateUpdateValue = (result.LateUpdateTime - lowPassLateUpdateValue) * lowPassFactor + lowPassLateUpdateValue;
            lowPassRenderingValue = (result.RenderingTime - lowPassRenderingValue) * lowPassFactor + lowPassRenderingValue;
            lowPassRenderTextureRenderingValue = (result.TextureRenderingTime - lowPassRenderTextureRenderingValue) * lowPassFactor + lowPassRenderTextureRenderingValue;


            GL.PushMatrix();
            GL.LoadOrtho();


            //1段目:バー表示
            var row1Top = GetMarginTop(1);

            //2段目:Update
            var row2Top = GetMarginTop(2);
            //3段目:LateUpdate
            var row3Top = GetMarginTop(3);
            //4段目:FixedUpdate
            var row4Top = GetMarginTop(4);
            //5段目:RenderingTime
            var row5Top = GetMarginTop(5);
            //6段目:TextureRenderingTime
            var row6Top = GetMarginTop(6);

            //バー
            this.barMaterial.SetPass(0);
            GL.Begin(GL.QUADS);

            GLHelper.DrawBar(new Vector3(this.barMarginLeft, screenSize.y - row1Top), Color.black, this.barMaxWidth, RowHeight, this.screenSize);//黒背景
            var xPosition = GLHelper.DrawBar(new Vector3(this.barMarginLeft, screenSize.y - row1Top), UpdateColor, (float)(lowPassUpdateValue / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            xPosition = GLHelper.DrawBar(new Vector3(xPosition, screenSize.y - row1Top), LateUpdateColor, (float)(lowPassLateUpdateValue / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            xPosition = GLHelper.DrawBar(new Vector3(xPosition, screenSize.y - row1Top), FixedUpdateColor, (float)(lowPassFixedValue / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            xPosition = GLHelper.DrawBar(new Vector3(xPosition, screenSize.y - row1Top), RenderingColor, (float)(lowPassRenderingValue / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            xPosition = GLHelper.DrawBar(new Vector3(xPosition, screenSize.y - row1Top), TextureRenderingColor, (float)(lowPassRenderTextureRenderingValue / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);

            GL.End();

            //テキスト
            this.FontCharacterInitialize();
            this.builtinFont.material.SetPass(0);
            GL.Begin(GL.QUADS);
            //Update
            var lastXposition = this.characterHelper.DrawString("Update:", new Vector3(this.fontMarginLeft, screenSize.y - row2Top), UpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(lowPassUpdateValue, new Vector3(lastXposition, screenSize.y - row2Top), UpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row2Top), UpdateColor, this.textScale);

            //LateUpdate
            lastXposition = this.characterHelper.DrawString("LateUpdate:", new Vector3(this.fontMarginLeft, screenSize.y - row3Top), LateUpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(lowPassLateUpdateValue, new Vector3(lastXposition, screenSize.y - row3Top), LateUpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row3Top), LateUpdateColor, this.textScale);

            //FixedUpdate
            lastXposition = this.characterHelper.DrawString("FixedUpdate:", new Vector3(this.fontMarginLeft, screenSize.y - row4Top), FixedUpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(lowPassFixedValue, new Vector3(lastXposition, screenSize.y - row4Top), FixedUpdateColor, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row4Top), FixedUpdateColor, this.textScale);

            //Rendering
            lastXposition = this.characterHelper.DrawString("Rendering:", new Vector3(this.fontMarginLeft, screenSize.y - row5Top), RenderingColor, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(lowPassRenderingValue, new Vector3(lastXposition, screenSize.y - row5Top), RenderingColor, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row5Top), RenderingColor, this.textScale);

            //TextureRendering
            lastXposition = this.characterHelper.DrawString("TextureRendering:", new Vector3(this.fontMarginLeft, screenSize.y - row6Top), TextureRenderingColor, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(lowPassRenderTextureRenderingValue, new Vector3(lastXposition, screenSize.y - row6Top), TextureRenderingColor, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row6Top), TextureRenderingColor, this.textScale);

            GL.End();

            GL.PopMatrix();
        }


        /// <summary>
        /// 指定した行のy座標を返します。
        /// </summary>
        /// <param name="row">行番号</param>
        private float GetMarginTop(int row)
        {
            return row * (RowHeight + 7);//行の高さ+ 行間の余白
        }
    }
}