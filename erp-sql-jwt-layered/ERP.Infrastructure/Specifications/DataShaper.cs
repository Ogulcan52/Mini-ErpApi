using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ERP.Application.Sepicifications;

namespace ERP.Infrastructure.Specifications
{
    public class DataShaper<T>:IDataShaper<T>
    {

        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchData(entities, requiredProperties);
        }

        public ExpandoObject ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchDataForEntity(entity, requiredProperties);
        }

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var properties = new List<PropertyInfo>();
            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var field in fields)
                {
                    var property = typeof(T).GetProperty(field.Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                        properties.Add(property);
                }
            }
            else
            {
                properties.AddRange(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));
            }
            return properties;
        }

        private IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            foreach (var entity in entities)
            {
                yield return FetchDataForEntity(entity, requiredProperties);
            }
        }

        private ExpandoObject FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ExpandoObject();
            foreach (var property in requiredProperties)
            {
                var value = property.GetValue(entity);
                ((IDictionary<string, object?>)shapedObject).Add(property.Name, value);
            }
            return shapedObject;
        }
    }
}
