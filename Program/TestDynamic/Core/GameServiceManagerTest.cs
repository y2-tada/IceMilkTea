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

using System.Collections;
using IceMilkTea.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IceMilkTeaTestDynamic.Core
{
    #region テスト確認用クラス
    /*
     * テストで用いるクラスは以下の継承構造になります。
     * 
     * GameService（IceMilkTeaにおけるサービスの抽象クラス）
     * ├ ServiceBaseA（ユーザー定義クラスの最基底サービスクラスでありキーとなるクラス）
     * │  └ServiceA_1_0
     * │   ├ ServiceA_2_0
     * │   └ ServiceA_2_1
     * │
     * └ ServiceBaseB（ユーザー定義クラスの最基底サービスクラスでありキーとなるクラス）
     *   └ ServiceB_1_0
     *      ├ ServiceB_2_0
     *      └ ServiceB_2_1
     * 
     * IceMilkTeaでは "GameServiceクラスを直接継承したクラス" がキーとなるクラスとなり
     * そのクラスを継承したすべての派生クラスは、キーとなるクラスが重複して登録されない制約が付きます。
     */

    /// <summary>
    /// サービスAの基本テストクラスです
    /// </summary>
    public class ServiceBaseA : GameService
    {
    }



    /// <summary>
    /// サービスAの基本クラスから派生したサービスA1クラスです
    /// </summary>
    public class ServiceA_1_0 : ServiceBaseA
    {
    }



    /// <summary>
    /// サービスAクラスから更に派生したサービスA2クラスです
    /// </summary>
    public class ServiceA_2_0 : ServiceA_1_0
    {
    }



    /// <summary>
    /// サービスAクラスからもう一つ派生したサービスA2の二つ目のクラスです
    /// </summary>
    public class ServiceA_2_1 : ServiceA_1_0
    {
    }



    /// <summary>
    /// サービスBの基本テストクラスです
    /// </summary>
    public class ServiceBaseB : GameService
    {
    }



    /// <summary>
    /// サービスBの基本クラスから派生したサービスB1クラスです
    /// </summary>
    public class ServiceB_1_0 : ServiceBaseB
    {
    }



    /// <summary>
    /// サービスBクラスから更に派生したサービスB2クラスです
    /// </summary>
    public class ServiceB_2_0 : ServiceB_1_0
    {
    }



    /// <summary>
    /// サービスBクラスからもう一つ派生したサービスB2の二つ目のクラスです
    /// </summary>
    public class ServiceB_2_1 : ServiceB_1_0
    {
    }
    #endregion



    /// <summary>
    /// IceMilkTeaの GameServiceManager をテストするクラスです
    /// </summary>
    public class ServiceManagerTest
    {
        // メンバ変数定義
        private GameServiceManager manager;



        /// <summary>
        /// テストの開始準備を行います
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            // マネージャの起動では例外さえ出なければ良いとする
            Assert.DoesNotThrow(() =>
            {
                // 空ゲームメインで動作を上書きしてサービスマネージャを取得する
                GameMain.OverrideGameMain(ScriptableObject.CreateInstance<EmptyGameMain>());
                manager = GameMain.Current.ServiceManager;
            });
        }


        /// <summary>
        /// テストの終了処理を行います
        /// </summary>
        [OneTimeTearDown]
        public void Celanup()
        {
            // ここでは何もしない
        }


        /// <summary>
        /// サービスの追加をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(10)]
        public IEnumerator AddServiceTest()
        {
            // サービスAクラスは末端の2系サービスを追加し、別のクラスが登録出来ないことを確認
            // サービスBクラスは中間の1系サービスを追加し、基本クラス及び末端クラスの登録が出来ない事を確認


            // サービスA2_0、サービスB1_0を登録できる事を確認する
            Assert.DoesNotThrow(() => manager.AddService(new ServiceA_2_0()));
            Assert.DoesNotThrow(() => manager.AddService(new ServiceB_1_0()));


            // サービスAのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseA()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_1_0()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_1()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_0())); // 重複確認


            // サービスBのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseB()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_0()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_1()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_1_0())); // 重複確認


            // フレームを進める
            yield return null;


            // 後続テストの為にサービスを削除する（削除自体のテストは別途タイミングで行う）
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();


            // フレームを進める
            yield return null;
        }


        /// <summary>
        /// サービスの追加（TryAddService）をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(20)]
        public IEnumerator TryAddServiceTest()
        {
            // TryAddServiceは、基本の振る舞いはAddServiceと同じで
            // リザルトが例外ではなく戻り値の確認となる


            // サービスA2_0、サービスB1_0を登録できる事を確認する
            Assert.IsTrue(manager.TryAddService(new ServiceA_2_0()));
            Assert.IsTrue(manager.TryAddService(new ServiceB_1_0()));


            // サービスAのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.IsFalse(manager.TryAddService(new ServiceBaseA()));
            Assert.IsFalse(manager.TryAddService(new ServiceA_1_0()));
            Assert.IsFalse(manager.TryAddService(new ServiceA_2_1()));
            Assert.IsFalse(manager.TryAddService(new ServiceA_2_0())); // 重複確認


            // サービスBのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.IsFalse(manager.TryAddService(new ServiceBaseB()));
            Assert.IsFalse(manager.TryAddService(new ServiceB_2_0()));
            Assert.IsFalse(manager.TryAddService(new ServiceB_2_1()));
            Assert.IsFalse(manager.TryAddService(new ServiceB_1_0())); // 重複確認


            // フレームを進める
            yield return null;


            // 後続テストの為にサービスを削除する（削除自体のテストは別途タイミングで行う）
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();


            // フレームを進める
            yield return null;
        }


        /// <summary>
        /// サービスの取得をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(30)]
        public IEnumerator GetServiceTest()
        {
            // サービスAクラスは末端の2系サービスを追加し、GameServiceとサービスA2_1以外でサービスの取得が出来ることを確認
            // サービスBクラスは中間の1系サービスを追加し、GameServiceとサービスB2系以外でサービスの取得が出来ることを確認


            // この時点ではまだ全サービスの取得が出来ないことを確認する
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>());


            // サービスA2_0、サービスB1_0を登録する
            manager.AddService(new ServiceA_2_0());
            manager.AddService(new ServiceB_1_0());


            // 通常は追加された直後のサービスは起動していないが、取得は可能であることは保証しているべきなので、サービスA2_0、サービスB1_0の取得系がちゃんと動作するか確認
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>()); // 最基底クラスでは取得は出来ない
            Assert.DoesNotThrow(() => manager.GetService<ServiceBaseA>()); // A2_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceA_1_0>()); // A2_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceA_2_0>()); // A2_0ご本人様
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>()); // A2_0とは関係のないサービス
            Assert.DoesNotThrow(() => manager.GetService<ServiceBaseB>()); // B1_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceB_1_0>()); // B1_0ご本人様
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>()); // B1_0を継承しているがB1_0から見たら関係ないサービス


            // フレームを進める
            yield return null;


            // サービスが起動直後も、起動前と同じ挙動になるか、期待動作の確認
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>()); // 最基底クラスでは取得は出来ない
            Assert.DoesNotThrow(() => manager.GetService<ServiceBaseA>()); // A2_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceA_1_0>()); // A2_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceA_2_0>()); // A2_0ご本人様
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>()); // A2_0とは関係のないサービス
            Assert.DoesNotThrow(() => manager.GetService<ServiceBaseB>()); // B1_0の継承元
            Assert.DoesNotThrow(() => manager.GetService<ServiceB_1_0>()); // B1_0ご本人様
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>()); // B1_0を継承しているがB1_0から見たら関係ないサービス


            // サービスを削除する
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();


            // 通常は削除された直後のサービスはまだ停止していないが、シャットダウン予定としてマークされ取得されない事を保証しているべきなので、すべてのサービスが取得できないか確認
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>());


            // フレームを進める
            yield return null;


            // 最後にフレーム経過してもサービスの取得が出来ないことを確認する
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_1_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>());
        }


        /// <summary>
        /// サービスの取得（TryGetService）をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(40)]
        public IEnumerator TryGetServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの削除をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(50)]
        public IEnumerator RemoveServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスのアクティブ設定をのテストをします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(60)]
        public IEnumerator ActiveDeactiveTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの起動停止の処理が想定タイミングで行われるかをテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        public IEnumerator StartupAndShutdownTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }
    }
}