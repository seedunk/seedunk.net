 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Seedunk.Net.IModels
{
    public static class IModelExtensions {

        public static T ToModel<T>(this IModel model) where T : class {
             
            PropertyInfo[] infos= model.GetType().GetProperties(); 
            T obj = System.Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            { 
                if (model.GetType().GetProperty(property.Name) != null) {

                    var modelProperty = model.GetType().GetProperty(property.Name);
                    if(modelProperty.CanWrite&&modelProperty.CanRead)
                        modelProperty.SetValue(obj, modelProperty.GetValue(model));
                     
                }    
            } 
            return obj;
         }
         

    }
    public interface IModel
    {
        string ModelName { get; }
        string PrimaryPropertyName { get; }
         
        /// <summary>
        /// 实体转字典
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> ToDictionary();

        /// <summary>
        /// 实体绑定字典
        /// </summary>
        /// <param name="value"></param>
        void BindDictionary(Dictionary<string, object> value);
    }
}
