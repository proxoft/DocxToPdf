﻿using System;
using System.IO;
using System.Linq;
using CommandLine;
using Proxoft.DocxToPdf.Runner.Commands;

namespace Proxoft.DocxToPdf.Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool close = false;
            string[] verb = args;

            do
            {
                close = Parser.Default
                    .ParseArguments<ConvertCommand, QuitCommand>(verb)
                    .MapResult(
                        (ConvertCommand cc) => Convert(cc),
                        (QuitCommand qc) => true,
                        err => false);

                if (!close)
                {
                    Console.Write("$  ");
                    verb = Console.ReadLine().Split(" ").ToArray();
                }
            } while (!close);
        }

        private static bool Convert(ConvertCommand command)
        {
            try
            {
                ExecuteConvert(command.DocxPath, command.PdfOutputPath);

                Console.WriteLine("Done...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Convert failed");
                Console.WriteLine(ex);
            }

            return !command.Continue;
        }

        private static void ExecuteConvert(string docxFilePath, string pdfOutputFilePath)
        {
            using var docxStream = File.Open(docxFilePath, FileMode.Open, FileAccess.Read);
            var pdfGenerator = new PdfGenerator();
            var pdf = pdfGenerator.GenerateAsByteArray(docxStream);
            File.WriteAllBytes(pdfOutputFilePath, pdf);
        }
    }
}
