using System.Text;

namespace AndromedaTM1Sharp
{
    public partial class TM1RestAPI
    {
        /// <summary>
        /// Queries a cell using the specified TM1SharpConfig, cube name, and element values.
        /// </summary>
        /// <param name="tm1">The TM1SharpConfig instance.</param>
        /// <param name="cubeName">The name of the cube.</param>
        /// <param name="d1">Dimension 1.</param>
        /// <param name="e1">Element 1.</param>
        /// <param name="d2">Dimension 2.</param>
        /// <param name="e2">Element 2.</param>
        /// <param name="d3">Dimension 3 (optional).</param>
        /// <param name="e3">Element 3 (optional).</param>
        /// <param name="d4">Dimension 4 (optional).</param>
        /// <param name="e4">Element 4 (optional).</param>
        /// <param name="d5">Dimension 5 (optional).</param>
        /// <param name="e5">Element 5 (optional).</param>
        /// <param name="d6">Dimension 6 (optional).</param>
        /// <param name="e6">Element 6 (optional).</param>
        /// <param name="d7">Dimension 7 (optional).</param>
        /// <param name="e7">Element 7 (optional).</param>
        /// <param name="d8">Dimension 8 (optional).</param>
        /// <param name="e8">Element 8 (optional).</param>
        /// <param name="d9">Dimension 9 (optional).</param>
        /// <param name="e9">Element 9 (optional).</param>
        /// <param name="d10">Dimension 10 (optional).</param>
        /// <param name="e10">Element 10 (optional).</param>
        /// <returns>The cell value as a string.</returns>
        public static async Task<string?> QueryCellAsync(TM1SharpConfig tm1, string cubeName, 
            string d1, string e1, 
            string d2, string e2, 
            string d3 = "", string e3 = "", 
            string d4 = "", string e4 = "", 
            string d5 = "", string e5 = "", 
            string d6 = "", string e6 = "", 
            string d7 = "", string e7 = "", 
            string d8 = "", string e8 = "", 
            string d9 = "", string e9 = "", 
            string d10 = "", string e10 = "")
        {
            var d = new Dictionary<string, string>();

            if (d3 != "" && e3 != "") d.Add(d3, e3);
            if (d4 != "" && e4 != "") d.Add(d4, e4);
            if (d5 != "" && e5 != "") d.Add(d5, e5);
            if (d6 != "" && e6 != "") d.Add(d6, e6);
            if (d7 != "" && e7 != "") d.Add(d7, e7);
            if (d8 != "" && e8 != "") d.Add(d8, e8);
            if (d9 != "" && e9 != "") d.Add(d9, e9);
            if (d10 != "" && e10 != "") d.Add(d10, e10);

            var mdx = new StringBuilder();

            string mdxQueryMember = "[" + d1 + "].[" + e1 + "]";

            mdx.Append("SELECT {" + mdxQueryMember + "} ON 0, {{HEAD(TM1FILTERBYPATTERN(TM1SubsetAll([" + d2 + "]),\\\"" + e2 + "\\\"),1)}} ON 1 FROM [" + cubeName + "]");

            var whereClause = new StringBuilder();

            foreach (var k in d.Keys)
            {
                whereClause.Append("[" + k + "].[" + d[k] + "],");
            }

            if (whereClause.Length > 0)
            {
                whereClause.Remove(whereClause.Length - 1, 1);

                mdx.Append(" WHERE (" + whereClause.ToString() + ")");
            }

            var model = CellsetJSONParser.ParseIntoObject(await QueryMDXAsync(tm1, mdx.ToString()));

            if (model?.Cells?.Count != 1) throw new ArgumentException("Expected return of 1 cell, check parameters.");

            return model?.Cells?[0]?.Value?.ToString();
        }
    }
}
