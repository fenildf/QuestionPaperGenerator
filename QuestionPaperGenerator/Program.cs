﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace QuestionPaperGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Opening workbook...");

			var workbook = new XSSFWorkbook(File.OpenRead("question.xlsx"));
			var sheet = workbook.GetSheet("Sheet1");

			//read question from sheet1
			var questions = new List<QuestionItem>();
			for (int i = 0; i < sheet.LastRowNum + 1; i++)
			{
				var row = sheet.GetRow(i);
				var questionItem = new QuestionItem
				{
					Answer = row.Cells[0].ToString(),
					Id = row.Cells[1].ToString(),
					Question = row.Cells[2].ToString(),
					Options = new List<string>(6)
				};
				for (int columnIndex = 3; columnIndex < row.Cells.Count; columnIndex++)
				{
					questionItem.Options.Add(row.Cells[columnIndex].ToString());
				}
				questions.Add(questionItem);
			}

			//generate papers and answers
			for (int i = 0; i < 60; i++)
			{
				var exportWorkbook = new XSSFWorkbook();
				var sheet1 = exportWorkbook.CreateSheet(i.ToString());
				//create header
				var headerRow = sheet1.CreateRow(0);
				headerRow.CreateCell(0).SetCellValue("填写答案");
				headerRow.CreateCell(1).SetCellValue("编号");
				headerRow.CreateCell(2).SetCellValue("问题");
				int maxOptionCount = questions.Max(q => q.Options.Count);
				for (int optionIndex = 0; optionIndex < maxOptionCount; optionIndex++)
				{
					headerRow.CreateCell(optionIndex + 3).SetCellValue("选项" + (char) ('A' + optionIndex));
				}

				var rnd = new Random();
				var randomQuestions = questions.OrderBy(q => rnd.Next()).ToList();
				for (int rowIndex = 0; rowIndex < randomQuestions.Count; rowIndex++)
				{
					var newRow = sheet1.CreateRow(rowIndex + 1);
					var randomQuestion = randomQuestions[rowIndex];
					newRow.CreateCell(0, CellType.String).SetCellValue("");
					newRow.CreateCell(1, CellType.String).SetCellValue(randomQuestion.Id);
					newRow.CreateCell(2, CellType.String).SetCellValue(randomQuestion.Question);
					for (int columnIndex = 0; columnIndex < randomQuestion.Options.Count; columnIndex++)
					{
						newRow.CreateCell(columnIndex + 3, CellType.String).SetCellValue(randomQuestion.Options[columnIndex]);
					}
				}
				sheet1.SetColumnHidden(1, true);
				sheet1.CreateFreezePane(1, 1);
				exportWorkbook.SetActiveSheet(0);
				
				Directory.CreateDirectory("Papers");
				var filePath = @"Papers\" + i + ".xlsx";
				using (var fs = new FileStream(filePath, FileMode.Create))
				{
					exportWorkbook.Write(fs);
				}
			}
		}
	}
}