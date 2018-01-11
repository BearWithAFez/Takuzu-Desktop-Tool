using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Takuzu_Solver_and_Generator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // Vars
        readonly Button[] playCells, solveCells;
        const int ATTEMPTS = 20; 
        ConcurrentBag<string> solutions = new ConcurrentBag<string>();
        ConcurrentBag<string> puzzles = new ConcurrentBag<string>();
        ConcurrentBag<string> endStates = new ConcurrentBag<string>();
        enum SolveStyles { bruteForce, bruteForcePrune, human, hybrid }

        // INIT
        public MainWindow() {
            InitializeComponent();

            // Cells
            playCells = new Button[]{
                cellPlay0, cellPlay1, cellPlay2, cellPlay3, cellPlay4, cellPlay5, cellPlay6, cellPlay7, cellPlay8, cellPlay9,
                cellPlay10, cellPlay11, cellPlay12, cellPlay13, cellPlay14, cellPlay15, cellPlay16, cellPlay17, cellPlay18, cellPlay19,
                cellPlay20, cellPlay21, cellPlay22, cellPlay23, cellPlay24, cellPlay25, cellPlay26, cellPlay27, cellPlay28, cellPlay29,
                cellPlay30, cellPlay31, cellPlay32, cellPlay33, cellPlay34, cellPlay35
            };
            solveCells = new Button[]{
                cellSolve0, cellSolve1, cellSolve2, cellSolve3, cellSolve4, cellSolve5, cellSolve6, cellSolve7, cellSolve8, cellSolve9,
                cellSolve10, cellSolve11, cellSolve12, cellSolve13, cellSolve14, cellSolve15, cellSolve16, cellSolve17, cellSolve18, cellSolve19,
                cellSolve20, cellSolve21, cellSolve22, cellSolve23, cellSolve24, cellSolve25, cellSolve26, cellSolve27, cellSolve28, cellSolve29,
                cellSolve30, cellSolve31, cellSolve32, cellSolve33, cellSolve34, cellSolve35
            };
            tbCode_Play.Text = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            tbOutput_Play.AppendText("Welcome! \n");
            tbOutput_Sol.AppendText("Welcome! \n");
            tbOutput_Gen.AppendText("Welcome! \n");
            cbStyle_Sol.ItemsSource = Enum.GetValues(typeof(SolveStyles));
        }

        // GEN 
        private void BtnGenerate_Gen_Click(object sender, RoutedEventArgs e) {
            int attempts = 0;
            if (Int32.TryParse(tbDepth_Gen.Text, out int depth)) {
                if (Int32.TryParse(tbDemand_Gen.Text, out int demand)) {
                    for (puzzles = new ConcurrentBag<string>(); (puzzles.Count < demand) && (attempts < ATTEMPTS);) {
                        string puzzle = FindRandomPuzzle(depth);
                        if (puzzle == "noneFound") {
                            attempts++;
                            tbOutput_Gen.AppendText("Miss...\n");
                        } else if (!puzzles.Contains(puzzle)) {
                            puzzles.Add(puzzle);
                            tbOutput_Gen.AppendText("Hit!\n");
                        } else {
                            tbOutput_Gen.AppendText("Duplicate hit.\n");
                        }
                    }
                } else {
                    tbOutput_Gen.AppendText("Demand is not a valid int!\n");
                }
            } else {
                tbOutput_Gen.AppendText("Depth is not a valid int!\n");
            }
        }

        private void BtnSave_Gen_Click(object sender, RoutedEventArgs e) {
            //todo: implement after generate!
        }

        // SOL
        private void BtnSolve_Sol_Click(object sender, RoutedEventArgs e) {
            tbSolution_Sol.Text = "";     
            
            Solve(tbCode_Sol.Text, (SolveStyles) cbStyle_Sol.SelectedValue);
            tbOutput_Sol.AppendText(solutions.Count + " solution(s) found \n");
            foreach(var sol in solutions) tbSolution_Sol.AppendText(sol + ";");
            tbOutput_Sol.ScrollToEnd();
        }

        private void BtnFill_Sol_Click(object sender, RoutedEventArgs e) {
            if (ConvertCodeToGrid(tbCode_Sol.Text, solveCells)) {
                btnSolve_Sol.IsEnabled = true;
                tbOutput_Sol.AppendText("Cell fill succses! \n");
            } else {
                btnSolve_Sol.IsEnabled = false;
                EmptyGrid(solveCells);
                tbOutput_Sol.AppendText("Cell fill failed, instead just cleared...\n");
            }
        }

        // PLAY
        private void BtnCheck_Play_Click(object sender, RoutedEventArgs e) {
            var errors = CheckCodeIsSolution(tbCurrent_Play.Text);

            if (errors.Count == 0) {
                tbOutput_Play.AppendText("Congrats its all good!\n");
                foreach (var cell in playCells) cell.IsEnabled = false;
            } else {
                tbOutput_Play.AppendText("Whoops some errors found:\n");
                foreach (var error in errors) tbOutput_Play.AppendText(error + "\n");
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button) {
                var cell = "" + button.Content as string;
                button.Content = (cell == "") ? "0" : ((cell == "0") ? "1" : "0");
                tbCurrent_Play.Text = UpdateCode(playCells);
                tbOutput_Play.AppendText("Cell edited\n");
            }
        }

        private void BtnFill_Play_Click(object sender, RoutedEventArgs e) {
            if(ConvertCodeToGrid(tbCode_Play.Text, playCells)) {
                btnCheck_Play.IsEnabled = true;
                tbCurrent_Play.Text = tbCode_Play.Text;
                tbOutput_Play.AppendText("Cell fill succses! \n");
            } else {
                btnCheck_Play.IsEnabled = false;
                tbCurrent_Play.Text = "";
                EmptyGrid(playCells);
                tbOutput_Play.AppendText("Cell fill failed, instead just cleared...\n");
            }
        }

        // HELPERS
        /// <summary>
        /// Solves the puzzle in a certain way
        /// </summary>
        /// <param name="code">The puzzle</param>
        /// <param name="style">The method</param>
        private void Solve(string code, SolveStyles style) {
            solutions = new ConcurrentBag<string>();
            DateTime start = DateTime.Now;
            switch (style) {
                case SolveStyles.bruteForce:
                    TryBruteSolve(code, false, false);
                    break;
                case SolveStyles.bruteForcePrune:
                    TryBruteSolve(code, true, false);
                    break;
                case SolveStyles.human:
                    TryHumanSolve(code, false);
                    break;
                case SolveStyles.hybrid:
                    TryHumanSolve(code, true);
                    break;
                default:
                    // Unknown solve style
                    Console.WriteLine("Unknown style used?");
                    break;
            }
            Console.WriteLine(DateTime.Now.Subtract(start).TotalSeconds + "s used to calculate current solution");
        }

        /// <summary>
        /// Very quickly checks if the given code only has one solution
        /// </summary>
        /// <param name="code">The code to check</param>
        /// <returns></returns>
        private bool CheckUniqueSolution(string code) {
            solutions = new ConcurrentBag<string>();
            string solution = "" + code;
            do {
                code = "" + solution;
                solution = "" + HumanSolveItteration(code);
            }
            while (solution != code);
            if (QuickCheckIfErrorless(solution, true)) return true;
            TryBruteSolve(solution, true, true);
            if (solutions.Count >= 2 || solutions.Count == 0) return false;
            return true;
        }

        /// <summary>
        /// Recursive solver with pruning
        /// </summary>
        /// <param name="input">The code</param>
        /// <param name="prune">If pruning can be used</param>
        /// <param name="quick">If its just a quick check</param>
        private void TryBruteSolve(string input, bool prune, bool quick) {
            StringBuilder s = new StringBuilder(input);

            // End point?
            if (input.IndexOf('e') == -1) {
                if (QuickCheckIfErrorless(input.ToString(), true)) solutions.Add(input);
                return;
            }

            // Prune
            if(prune) if (!QuickCheckIfErrorless(input.ToString(), false)) return;

            // Brute Force Me
            int i = input.IndexOf('e');
            s[i] = '0';
            TryBruteSolve(s.ToString(), prune, quick);
            if (solutions.Count == 2 && quick) return;
            s[i] = '1';
            TryBruteSolve(s.ToString(), prune, quick);
        }
        
        /// <summary>
        /// Converts grid to code
        /// </summary>
        /// <param name="cells">The grid</param>
        /// <returns>the code</returns>
        private string UpdateCode(Button[] cells) {
            StringBuilder newCode = new StringBuilder("eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
            for (int i = 0; i < cells.Length; i++) {
                string cell = "" + cells[i].Content as string;
                newCode[i] = (cell == "") ? 'e' : cell[0];
            }
            return newCode.ToString();
        }

        /// <summary>
        /// Empties the grid
        /// </summary>
        /// <param name="grid">The grid</param>
        private void EmptyGrid(Button[] grid) {
            foreach (var c in grid) c.Content = "";
        }
        
        /// <summary>
        /// Converts a code to a grid
        /// </summary>
        /// <param name="code">The given code</param>
        /// <param name="grid">The to insert grid</param>
        /// <returns></returns>
        private bool ConvertCodeToGrid(string code, Button[] grid) {
            // Check if code is correct length
            if (code.Length != 36) return false;

            // Check if code has correct chars
            foreach (char c in code) if (!"01e".Contains(c)) return false;

            // All ok so fill
            for (int i = 0; i < code.Length; i++) {
                grid[i].Content = (code[i] == 'e') ? "" : "" + code[i]; // Content
                grid[i].IsEnabled = code[i] == 'e'; // Enable or disable cell
            }
            return true;
        }

        /// <summary>
        /// Checks if a given code already contains a error
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        private bool QuickCheckIfErrorless(string code, bool isSolution) {
            // Total length
            if (code.Length != 36) return false;

            // Endpoint
            if (code.IndexOf('e') != -1 && isSolution) return false;

            // # of 0 & 1
            if (code.Replace("1", "").Length < 18) return false;
            if (code.Replace("0", "").Length < 18) return false;

            // SubRows
            List<string> hors = new List<string>();
            List<string> vers = new List<string>();
            // Unsolvables
            hors.Add("0eee00");hors.Add("0ee100");hors.Add("0e1e00");hors.Add("0e1100");hors.Add("01ee00");hors.Add("01e100");hors.Add("011e00");
            hors.Add("00eee0");hors.Add("00ee10");hors.Add("00e1e0");hors.Add("00e110");hors.Add("001ee0");hors.Add("001e10");hors.Add("0011e0");
            hors.Add("1eee11");hors.Add("1ee011");hors.Add("1e0e11");hors.Add("1e0011");hors.Add("10ee11");hors.Add("10e011");hors.Add("100e11");
            hors.Add("11eee1");hors.Add("11ee01");hors.Add("11e0e1");hors.Add("11e001");hors.Add("110ee1");hors.Add("110e01");hors.Add("1100e1");

            vers.Add("0eee00");vers.Add("0ee100");vers.Add("0e1e00");vers.Add("0e1100");vers.Add("01ee00");vers.Add("01e100");vers.Add("011e00");
            vers.Add("00eee0");vers.Add("00ee10");vers.Add("00e1e0");vers.Add("00e110");vers.Add("001ee0");vers.Add("001e10");vers.Add("0011e0");
            vers.Add("1eee11");vers.Add("1ee011");vers.Add("1e0e11");vers.Add("1e0011");vers.Add("10ee11");vers.Add("10e011");vers.Add("100e11");
            vers.Add("11eee1");vers.Add("11ee01");vers.Add("11e0e1");vers.Add("11e001");vers.Add("110ee1");vers.Add("110e01");vers.Add("1100e1");

            for (int i = 0; i < 6; i++) {
                // Hors
                string subHor = code.Substring(i * 6, 6);
                if (hors.Contains(subHor)) return false; // Duplicates
                if (subHor.Replace("0", "").Length < 3) return false; // #0's
                if (subHor.Replace("1", "").Length < 3) return false; // #1's
                if (subHor.Contains("000")) return false; // 0 streak
                if (subHor.Contains("111")) return false; // 1 streak
                if(isSolution)hors.Add(subHor);

                // Hors
                string subVer = "" + code[i] + code[i + 6] + code[i + 12] + code[i + 18] + code[i + 24] + code[i + 30];
                if (vers.Contains(subVer)) return false; // Duplicates
                if (subVer.Replace("0", "").Length < 3) return false; // #0's
                if (subVer.Replace("1", "").Length < 3) return false; // #1's
                if (subVer.Contains("000")) return false; // 0 streak
                if (subVer.Contains("111")) return false; // 1 streak
                if(isSolution)vers.Add(subVer);
            }

            return true;
        }

        /// <summary>
        /// Checks if a code is a solution and returns all errors in a List
        /// </summary>
        /// <param name="code">Code to check</param>
        /// <returns>The List with all errors</returns>
        private List<string> CheckCodeIsSolution(string code) {
            List<string> errors = new List<string>();

            // Length
            if (code.Length != 36) {
                errors.Add("Code has wrong length..." + code.Length);
                return errors;
            }

            // Filled
            if (code.IndexOf('e') != -1) {
                errors.Add("Code has empty spots...");
                return errors;
            }

            // Duplicates
            List<string> hors = new List<string>();
            List<string> vers = new List<string>();
            for (int i = 0; i < 6; i++) {
                // Hors
                string subHor = code.Substring(i * 6, 6);
                if (hors.Contains(subHor)) errors.Add("Duplicate sub found " + subHor);
                hors.Add(subHor);

                // Vers
                string subVer =" " + code[i] + code[i + 6] + code[i + 12] + code[i + 18] + code[i + 24] + code[i + 30];
                if (vers.Contains(subVer)) errors.Add("Duplicate sub found " + subVer);
                vers.Add(subVer);
            }

            // Count
            foreach (var sub in hors.Concat(vers)) if ((sub.Length - sub.Replace("0", "").Length) != 3) errors.Add("Not equal 0's and 1's found in " + sub);

            // Chains
            foreach (var sub in hors.Concat(vers)) if (sub.Contains("000") || sub.Contains("111")) errors.Add("Chain found in " + sub);

            // Return errors
            return errors;
        }

        /// <summary>
        /// Attempts to solve the puzzle as a human would
        /// </summary>
        /// <param name="input">the puzzle</param>
        /// <param name="hybrid">if the solver can use BruteForcePruning when stuck</param>
        private void TryHumanSolve(string input, bool hybrid) {
            string solution = "" + input;
            do {
                input = "" + solution;
                solution = ""+ HumanSolveItteration(input);
            }
            while (solution != input);
            if (QuickCheckIfErrorless(solution, true)) solutions.Add(solution);
            else if (hybrid) {
                TryBruteSolve(solution, true, false);
            }
        }

        /// <summary>
        /// Does a single itteration of the human solve
        /// </summary>
        /// <param name="code">The code to solve</param>
        /// <returns></returns>
        private string HumanSolveItteration(string code) {
            // Invalid
            if (code.Length != 36) return code;
            if (!code.Contains('e')) return code;
            
            // Hor
            List<string> solvedSubs = new List<string>();
            string[] newCode = new string[6];

            for (int i = 0; i < 6; i++) {
                string sub = code.Substring(i * 6, 6);
                if (!sub.Contains('e')) solvedSubs.Add(sub);
            }
            
            for (int i = 0; i < 6; i++) {                
                StringBuilder sub = new StringBuilder(code.Substring(i * 6, 6));
                sub = new StringBuilder(HumanSolveScan(sub, solvedSubs));
                newCode[i] = sub.ToString();
            }
            StringBuilder newCodeString = new StringBuilder(String.Join("", newCode));

            // vers
            solvedSubs = new List<string>();
            for (int i = 0; i < 6; i++) {
                string sub = "" + newCodeString[i] + newCodeString[i + 6] + newCodeString[i + 12] + newCodeString[i + 18] + newCodeString[i + 24] + newCodeString[i + 30];
                if (!sub.Contains('e')) solvedSubs.Add(sub);
            }

            for (int i = 0; i < 6; i++) {
                StringBuilder sub = new StringBuilder("" + newCodeString[i] + newCodeString[i + 6] + newCodeString[i + 12] + newCodeString[i + 18] + newCodeString[i + 24] + newCodeString[i + 30]);
                sub = new StringBuilder(HumanSolveScan(sub, solvedSubs));
                newCodeString[i] = sub[0];
                newCodeString[i+6] = sub[1];
                newCodeString[i+12] = sub[2];
                newCodeString[i+18] = sub[3];
                newCodeString[i+24] = sub[4];
                newCodeString[i+30] = sub[5];
            }

            return newCodeString.ToString();
        }

        /// <summary>
        /// Scans the subcode and sets all known vars
        /// </summary>
        /// <param name="sub">The code</param>
        /// <param name="solvedSubs">All known full subs</param>
        /// <returns></returns>
        private string HumanSolveScan(StringBuilder sub, List<string> solvedSubs) {
            if (sub.ToString().Contains('e')) {
                // identical edge rule
                if (sub[0] == sub[5] && sub[5] != 'e') sub[1] = sub[4] = (sub[0] == '1') ? '0' : '1';
                // double rule
                sub = sub.Replace("e00", "100");
                sub = sub.Replace("e11", "011");
                sub = sub.Replace("00e", "001");
                sub = sub.Replace("11e", "110");
                sub = sub.Replace("0e0", "010");
                sub = sub.Replace("1e1", "101");
                // anti rule
                if (sub.ToString() == "001eee") sub = new StringBuilder("001ee1");
                else if (sub.ToString() == "110eee") sub = new StringBuilder("110ee0");
                else if (sub.ToString() == "eee100") sub = new StringBuilder("1ee100");
                else if (sub.ToString() == "eee011") sub = new StringBuilder("0ee011");
                // duplicate rule
                sub = new StringBuilder(DuplicateRule(sub.ToString(), solvedSubs));
                // #0's - #1's
                if (sub.ToString().Replace("0", "").Length == 3) sub = sub.Replace("e", "1");
                else if (sub.ToString().Replace("1", "").Length == 3) sub = sub.Replace("e", "0");
            }
            return sub.ToString();
        }

        /// <summary>
        /// The duplicate rule is that if a sub is already used you know the other one
        /// </summary>
        /// <param name="sub">Sub to check</param>
        /// <param name="list">All full subs</param>
        /// <returns></returns>
        private string DuplicateRule(string sub, List<string> list) {
            if (sub.ToString().Replace("0", "").Length == 4) {
                if (sub.ToString().Replace("1", "").Length == 4) {
                    // make 2 possibilites
                    StringBuilder s1 = new StringBuilder(sub);
                    s1[s1.ToString().IndexOf('e')] = '0';
                    s1[s1.ToString().IndexOf('e')] = '1';
                    StringBuilder s2 = new StringBuilder(sub);
                    s2[s2.ToString().IndexOf('e')] = '1';
                    s2[s2.ToString().IndexOf('e')] = '0';

                    // return the one that isnt already used
                    if (list.Contains(s1.ToString())) return s2.ToString();
                    if (list.Contains(s2.ToString())) return s1.ToString();
                }
            }
            return sub;
        }

        private string FindRandomPuzzle(int depth) {
            // Make sure there are endStates avaible
            if(endStates.Count == 0) {
                solutions = new ConcurrentBag<string>();
                TryBruteSolve("eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", true, false);
                endStates = new ConcurrentBag<string>(solutions);
                solutions = new ConcurrentBag<string>();
            }

            // Pick a random endState
            string endState = endStates.ElementAt((new Random()).Next(endStates.Count));

            // TODO: Random recursief verwijderen van oplossing en dan checken of er 1 opl is 
            return null;
        }
    }
}