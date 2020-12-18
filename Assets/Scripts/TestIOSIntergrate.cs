using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.UI;

namespace BlueNoah.NativeIntergrate
{
    public struct SampleStruct
    {
        public int a;
        public long b;
        public float c;
        public double d;
        public string e;
        public int[] f;
        public string[] g;
    }

    public class TestIOSIntergrate : MonoBehaviour
    {
        [SerializeField]
        GameObject itemPrefab;

        [SerializeField]
        Text message;

        [SerializeField]
        Transform root;

        static Text msg;

        private void Awake()
        {
            CreateItem("sampleMethod1", sampleMethod1);
            CreateItem("sampleMethod2", () =>
            {
                sampleMethod2(111, 111.1f, 111.1, true, "abcdefg");
            });
            CreateItem("sampleMethod3", () =>
            {
                var intArr = new int[] { 111, 222, 333, 444, 555 };
                var strArr = new string[] { "a", "b", "c", "d", "e" };
                sampleMethod3(intArr, intArr.Length, strArr, strArr.Length);
            });
            CreateItem("sampleMethod4", () =>
            {
                sampleMethod4(cs_callback);
            });
            //TODO 这里好像有问题。
            CreateItem("sampleMethod5", () =>
            {
                GameObject gameObject = new GameObject("Sample_GameObject");
                IntPtr gameObjectPtr = (IntPtr)GCHandle.Alloc(gameObject);
                sampleMethod5(gameObjectPtr, cs_callback1);
            });
            CreateItem("sampleMethod5_1", () =>
            {
                var sampleStruct = new SampleStruct();
                sampleStruct.a = 111;
                sampleStruct.b = 222;
                sampleStruct.c = 3333333.1f;
                sampleStruct.d = 44444.1;
                sampleStruct.e = "abcdefg";
                sampleStruct.f = new int[] { 1, 2, 3, 4, 5, 6 };
                //TODO MarshalDirectiveException: Cannot marshal field 'g' of type 'SampleStruct'.
                sampleStruct.g = new string[] { "a", "b", "c", "d", "e", "f", "g" };
                sampleMethod5_1(sampleStruct);
            });
            msg = message;
        }

        GameObject CreateItem(string txt, Action action)
        {
            var go = Instantiate(itemPrefab);
            var button = go.GetComponentInChildren<Button>();
            var text = go.GetComponentInChildren<Text>();
            text.text = txt;
            button.onClick.AddListener(() =>
            {
                action?.Invoke();
            });
            go.transform.SetParent(root, false);
            go.SetActive(true);
            return go;
        }

        [DllImport("__Internal")]
        private static extern void sampleMethod1();
        [DllImport("__Internal")]
        private static extern void sampleMethod2(int val1, float val2, double val3, bool val4, string val5);
        [DllImport("__Internal")]
        private static extern void sampleMethod3(int[] intArr, int intArrSize, string[] strArr, int strArrSize);
        delegate void callback_delegate(int val);
        [DllImport("__Internal")]
        private static extern void sampleMethod4(callback_delegate callback);
        //回调函数，必须MonoPInvokeCallback并且是static
        [MonoPInvokeCallback(typeof(callback_delegate))]
        private static void cs_callback(int val)
        {
            Debug.Log("cs_callback : " + val);
            msg.text = val.ToString();
        }
        delegate void callback_delegate1(IntPtr gameObjectPtr, int val);
        [DllImport("__Internal")]
        private static extern void sampleMethod5(IntPtr gameObjectPtr, callback_delegate1 callback);
        [MonoPInvokeCallback(typeof(callback_delegate1))]
        private static void cs_callback1(IntPtr gameObjectPtr, int val)
        {
            GCHandle handle = (GCHandle)gameObjectPtr;
            GameObject gameObject = handle.Target as GameObject;
            handle.Free();
            Debug.Log("cs_callback : " + gameObject.name);
            msg.text = gameObject.name;
        }
        [DllImport("__Internal")]
        private static extern void sampleMethod5_1(SampleStruct data);
    }
}
