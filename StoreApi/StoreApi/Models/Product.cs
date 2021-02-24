using System.Data;
using System.Data.SqlClient;

namespace StoreApi.Models
{
    public class Product
    {        
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public void AddParam(IDbCommand comm, string paramName, object paramValue)
        {
            var param = comm.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue;
            comm.Parameters.Add(param);
        }

        internal SqlParameter AddOutParam(IDbCommand comm, string name, SqlDbType sqlDbType)
        {
            SqlParameter outPutParameter = new SqlParameter();
            outPutParameter.ParameterName = name;
            outPutParameter.SqlDbType = sqlDbType;
            outPutParameter.Direction = ParameterDirection.Output;
            comm.Parameters.Add(outPutParameter);
            return outPutParameter;
        }
    }
}
