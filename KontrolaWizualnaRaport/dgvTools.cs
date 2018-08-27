using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class dgvTools
    {


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
                Int32.TryParse(cell.Value.ToString(), out result);
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
    }
}
