using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/*
    아직 작업중입니다.

    //  Program.cs
    var dic = new Dictionary<string, object>();
    dic.Add("list", new List<int>() { 10, 20, 30 });
    dic.Add("one", 1);
    dic.Add("two", 2);
    dic.Add("arr", new List<string>() { "일", "이", "쓰리" });


    var subDic = new Dictionary<string, object>();
    subDic.Add("1", 1001);
    subDic.Add("2", 1002);
    dic.Add("dic", subDic);


    var test = new Test();
    Aegis.Data.Mapper.MappingWithDictionary(dic, test);


    //  Test.cs
    public class Test
    {
        public List<int> list { get; set; }
        public int one { get; set; }
        public string two { get; set; }
        public string[] arr { get; set; }
        public Dictionary<string, object> dic { get; set; }
    }
*/
namespace Aegis.Data
{
    public class Mapper
    {
        public static void MappingWithDictionary(Dictionary<string, object> source, object target)
        {
            foreach (KeyValuePair<string, object> kv in source)
            {
                //  target에 Key에 해당하는 Property가 있는지 확인
                var propertyInfo = target.GetType().GetProperty(kv.Key);
                if (propertyInfo != null)
                {
                    var propertyReturnType = propertyInfo.GetMethod.ReturnType;
                    dynamic sourceValue = Convert.ChangeType(kv.Value, kv.Value.GetType());


                    //  Dictionary
                    if (propertyReturnType.GetInterface(typeof(IDictionary<string, object>).Name) != null)
                    {
                        var dic = new Dictionary<string, object>();
                        propertyInfo.SetValue(target, dic);

                        MappingWithDictionary(sourceValue, dic);
                    }
                    //  Array
                    else if (propertyReturnType.IsArray)
                    {
                        if (sourceValue.GetType().GetGenericTypeDefinition() == typeof(List<>))
                        {
                            propertyInfo.SetValue(target, sourceValue.ToArray());
                        }
                    }
                    //  List
                    else if (propertyReturnType.GetInterface(typeof(IList<>).Name) != null)
                    {
                        propertyInfo.SetValue(target, sourceValue);
                    }
                    //  String
                    else if (propertyReturnType == typeof(String))
                        propertyInfo?.SetValue(target, sourceValue.ToString());
                    //  Etc
                    else
                        propertyInfo?.SetValue(target, sourceValue);
                }
                //  target이 Dictionary 객체일 경우
                else if (target.GetType().GetInterface(typeof(IDictionary<string, object>).Name) != null)
                {
                    var instance = (target as Dictionary<string, object>);
                    instance?.Add(kv.Key, kv.Value);
                }
            }
        }


        public static void Mapping(object source, object target)
        {
            var type = source.GetType();
            var checkDictionary = (type.GetInterface(typeof(IDictionary<string, object>).Name) != null);
            var checkArray = type.IsArray;
            var checkList = (type.GetInterface(typeof(IList<>).Name) != null);


            System.Diagnostics.Debug.WriteLine(String.Format("dic: {0}  arr: {1}  list: {2}", checkDictionary, checkArray, checkList));
        }
    }
}
