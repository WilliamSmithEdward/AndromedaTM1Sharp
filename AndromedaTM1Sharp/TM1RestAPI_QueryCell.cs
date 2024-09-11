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
        /// <param name="d11">Dimension 11 (optional).</param>
        /// <param name="e11">Element 11 (optional).</param>
        /// <param name="d12">Dimension 12 (optional).</param>
        /// <param name="e12">Element 12 (optional).</param>
        /// <param name="d13">Dimension 13 (optional).</param>
        /// <param name="e13">Element 13 (optional).</param>
        /// <param name="d14">Dimension 14 (optional).</param>
        /// <param name="e14">Element 14 (optional).</param>
        /// <param name="d15">Dimension 15 (optional).</param>
        /// <param name="e15">Element 15 (optional).</param>
        /// <param name="d16">Dimension 16 (optional).</param>
        /// <param name="e16">Element 16 (optional).</param>
        /// <param name="d17">Dimension 17 (optional).</param>
        /// <param name="e17">Element 17 (optional).</param>
        /// <param name="d18">Dimension 18 (optional).</param>
        /// <param name="e18">Element 18 (optional).</param>
        /// <param name="d19">Dimension 19 (optional).</param>
        /// <param name="e19">Element 19 (optional).</param>
        /// <param name="d20">Dimension 20 (optional).</param>
        /// <param name="e20">Element 20 (optional).</param>
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
            string d10 = "", string e10 = "",
            string d11 = "", string e11 = "",
            string d12 = "", string e12 = "",
            string d13 = "", string e13 = "",
            string d14 = "", string e14 = "",
            string d15 = "", string e15 = "",
            string d16 = "", string e16 = "",
            string d17 = "", string e17 = "",
            string d18 = "", string e18 = "",
            string d19 = "", string e19 = "",
            string d20 = "", string e20 = ""
        )
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
            if (d11 != "" && e11 != "") d.Add(d11, e11);
            if (d12 != "" && e12 != "") d.Add(d12, e12);
            if (d13 != "" && e13 != "") d.Add(d13, e13);
            if (d14 != "" && e14 != "") d.Add(d14, e14);
            if (d15 != "" && e15 != "") d.Add(d15, e15);
            if (d16 != "" && e16 != "") d.Add(d16, e16);
            if (d17 != "" && e17 != "") d.Add(d17, e17);
            if (d18 != "" && e18 != "") d.Add(d18, e18);
            if (d19 != "" && e19 != "") d.Add(d19, e19);
            if (d20 != "" && e20 != "") d.Add(d20, e20);

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

            var content = await QueryMDXAsync(tm1, mdx.ToString());

            var model = CellsetJSONParser.ParseIntoObject(content);

            var cellsetId = ParseCellsetId(content);

            DeleteCellset(tm1, cellsetId);

            if (model?.Cells?.Count != 1) throw new ArgumentException("Expected return of 1 cell, check parameters.");

            return model?.Cells?[0]?.Value?.ToString();
        }
    }
}
