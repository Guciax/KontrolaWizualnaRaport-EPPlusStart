using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class dgvTools
    {
        public static Color SwitchowColor(Color rowColor)
        {
            if (rowColor == Color.White) return Color.LightSteelBlue;
            return Color.White;
        }

        public static void MakeAlternatingRowsColors(DataGridView grid, int colIndex)
        {
            Color rowColor = Color.White;
            for (int r = 0; r < grid.Rows.Count; r++) 
            {
                if (r > 0)
                {
                    if (grid.Rows[r-1].Cells[colIndex].Value.ToString()!= grid.Rows[r].Cells[colIndex].Value.ToString())
                    {
                        rowColor = rowColor == Color.White ? Color.LightSteelBlue : Color.White;
                    }
                }

                foreach (DataGridViewCell cell in grid.Rows[r].Cells)
                {
                    cell.Style.BackColor = rowColor;
                }
            }
        }

        public static void ColumnsAutoSize(DataGridView grid, DataGridViewAutoSizeColumnMode mode)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = mode;
            }
        }

        public static void SumOfSelectedCells(DataGridView grid, Label lbl)
        {
            Int32 sum = 0;
            foreach (DataGridViewCell cell in grid.SelectedCells)
            {
                if (cell.Tag!=null)
                {
                    sum += GetCellIntValue(cell);
                }
            }
            lbl.Text = "Suma zaznaczonych: " + sum;
            lbl.Tag = sum.ToString();
        }

        public static Int32 GetCellIntValue(DataGridViewCell cell)
        {
            Int32 result = 0;
            if (cell.Value != null)
            {
                Int32.TryParse(cell.Value.ToString().Replace(" ",""), out result);
            }

            return result;
        }

        public static string GetCellStringValue(DataGridViewCell cell)
        {
            string result = "";
            if (cell.Value != null)
            {
                return cell.Value.ToString();
            }

            return result;
        }

        public static DataGridViewCell[] GetCellsForSameKeyValue(DataGridViewCell keyCell, DataGridView grid)
        {
            List<DataGridViewCell> cellsList = new List<DataGridViewCell>();
            int rowIndex = keyCell.RowIndex;
            int upLimit = rowIndex;
            int bottomLimit = rowIndex;
            int colIndex = keyCell.ColumnIndex;
            bool keepLooping = true;

            do
            {
                if (upLimit < grid.Rows.Count - 1)
                {
                    if (grid.Rows[upLimit + 1].Cells[colIndex].Value.ToString() == grid.Rows[upLimit].Cells[colIndex].Value.ToString())
                    {
                        upLimit++;
                    }
                    else
                    {
                        keepLooping = false;
                    }
                }
                else
                {
                    keepLooping = false;
                }
            } while (keepLooping);

            keepLooping = true;
            do
            {
                if (bottomLimit > 0)
                {
                    if (grid.Rows[bottomLimit - 1].Cells[colIndex].Value.ToString() == grid.Rows[bottomLimit].Cells[colIndex].Value.ToString())
                    {
                        bottomLimit--;
                    }
                    else
                    {
                        keepLooping = false;
                    }
                }
                else
                {
                    keepLooping = false;
                }
            } while (keepLooping);

            for (int r = bottomLimit; r <= upLimit; r++)
            {
                foreach (DataGridViewCell cell in grid.Rows[r].Cells)
                {
                    if (cell.Tag != null & !cell.OwningColumn.HeaderText.ToLower().Contains("dzien") & !cell.OwningColumn.HeaderText.ToLower().Contains("zmiana") & !cell.OwningColumn.HeaderText.ToLower().Contains("tydz"))
                    {
                        cellsList.Add(cell);
                    }
                }
            }

            return cellsList.ToArray();
        }

        public static void SetRowColor(DataGridViewRow row, Color backColor, Color? foreColor = null)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = backColor;
                if (!foreColor.HasValue)
                {
                    foreColor = Color.Black;
                }
                cell.Style.ForeColor = (Color)foreColor;
            }
        }

        public static DataTable AgregateCellsTables(DataGridView grid, DataGridViewCell startCell, string[] colNameWithTags)
        {
            DataTable result = null;
            
            if (startCell.Tag != null)
            {
                return (DataTable)startCell.Tag;
            }
            if (startCell.Value == null) return null;
            string startCellValue = startCell.Value.ToString();
            for (int r = startCell.RowIndex; r >= 0; r--)
            {
                if (grid.Rows[r].Cells[startCell.ColumnIndex].Value.ToString() != startCellValue) break;
                foreach (var colName in colNameWithTags)
                {
                    DataGridViewCell cell = grid.Rows[r].Cells[colName];
                    if (cell.Tag == null) continue;
                    DataTable cellTable = (DataTable)cell.Tag;
                    if (result == null)
                    {
                        result = cellTable.Clone();
                    }

                    foreach (DataRow row in cellTable.Rows)
                    {
                        result.Rows.Add(row.ItemArray);
                    }
                }
                //foreach (DataGridViewCell cell in grid.Rows[r].Cells)
                //{
                //    if (cell.Tag == null) continue;
                //    if (!colNameWithTags.Contains(cell.OwningColumn.Name)) continue;
                //    DataTable cellTable = (DataTable)cell.Tag;
                //    if (result==null) {
                //        result = cellTable.Clone();
                //    }

                //    foreach (DataRow row in cellTable.Rows)
                //    {
                //        result.Rows.Add(row.ItemArray);
                //    }
                //}
            }
            if (startCell.RowIndex == grid.Rows.Count - 1) return result;

            for(int r = startCell.RowIndex + 1;r<grid.Rows.Count; r++)
            {
                if (grid.Rows[r].Cells[startCell.ColumnIndex].Value.ToString() != startCellValue) break;
                foreach (var colName in colNameWithTags)
                {
                    DataGridViewCell cell = grid.Rows[r].Cells[colName];
                    if (cell.Tag == null) continue;
                    DataTable cellTable = (DataTable)cell.Tag;
                    if (result == null)
                    {
                        result = cellTable.Clone();
                    }

                    foreach (DataRow row in cellTable.Rows)
                    {
                        result.Rows.Add(row.ItemArray);
                    }
                }
                //foreach (DataGridViewCell cell in grid.Rows[r].Cells)
                //{
                //    if (cell.Tag == null) continue;
                //    if (!colNameWithTags.Contains(cell.OwningColumn.Name)) continue;
                //    DataTable cellTable = (DataTable)cell.Tag;
                //    if (result == null)
                //    {
                //        result = cellTable.Clone();
                //    }

                //    foreach (DataRow row in cellTable.Rows)
                //    {
                //        result.Rows.Add(row.ItemArray);
                //    }
                //}
            }

            return result;
        }
    }
}
