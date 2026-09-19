using System;
using System.Diagnostics;

namespace ExaminationSystem
{
    #region 1. Answer Class
    public class Answer : ICloneable
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = string.Empty;

        public Answer() { }

        public Answer(int answerId, string answerText)
        {
            AnswerId = answerId;
            AnswerText = answerText ?? string.Empty;
        }

        public object Clone()
        {
            return new Answer(AnswerId, AnswerText);
        }

        public override string ToString()
        {
            return $"{AnswerId}. {AnswerText}";
        }
    }
    #endregion

    #region 2. Question Base and Derived Classes
    public abstract class Question : ICloneable, IComparable<Question>
    {
        public string Header { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public double Mark { get; set; }
        public Answer[] AnswerList { get; set; } = Array.Empty<Answer>();
        public Answer RightAnswer { get; set; } = new Answer();
        public Answer UserAnswer { get; set; } = new Answer();

        public Question() { }

        public Question(string header, string body, double mark)
        {
            Header = header ?? string.Empty;
            Body = body ?? string.Empty;
            Mark = mark;
        }

        public abstract void CreateQuestion();

        public object Clone()
        {
            Question cloned = (Question)this.MemberwiseClone();
            if (AnswerList != null)
            {
                cloned.AnswerList = new Answer[AnswerList.Length];
                for (int i = 0; i < AnswerList.Length; i++)
                {
                    cloned.AnswerList[i] = (Answer)AnswerList[i].Clone();
                }
            }
            if (RightAnswer != null)
                cloned.RightAnswer = (Answer)RightAnswer.Clone();
            if (UserAnswer != null)
                cloned.UserAnswer = (Answer)UserAnswer.Clone();

            return cloned;
        }

        public int CompareTo(Question? other)
        {
            if (other == null) return 1;
            return Mark.CompareTo(other.Mark);
        }

        public override string ToString()
        {
            return $"{Header}\tMark: {Mark}\n{Body}";
        }
    }

    public class TFQuestion : Question
    {
        public TFQuestion() { }

        public TFQuestion(string header, string body, double mark) 
            : base(header, body, mark)
        {
        }

        public override void CreateQuestion()
        {
            AnswerList = new Answer[2]
            {
                new Answer(1, "True"),
                new Answer(2, "False")
            };

            int rightAnswerId;
            do
            {
                Console.Write("Enter Right Answer Id (1 for True, 2 for False): ");
            } while (!int.TryParse(Console.ReadLine(), out rightAnswerId) || (rightAnswerId != 1 && rightAnswerId != 2));

            RightAnswer = AnswerList[rightAnswerId - 1];
        }
    }

    public class McqQuestion : Question
    {
        public McqQuestion() { }

        public McqQuestion(string header, string body, double mark) 
            : base(header, body, mark)
        {
        }

        public override void CreateQuestion()
        {
            AnswerList = new Answer[4];
            Console.WriteLine("Enter MCQ Choices:");
            for (int i = 0; i < 4; i++)
            {
                Console.Write($"Choice {i + 1}: ");
                string text = Console.ReadLine() ?? string.Empty;
                AnswerList[i] = new Answer(i + 1, text);
            }

            int rightAnswerId;
            do
            {
                Console.Write("Enter Right Answer Id (1 to 4): ");
            } while (!int.TryParse(Console.ReadLine(), out rightAnswerId) || rightAnswerId < 1 || rightAnswerId > 4);

            RightAnswer = AnswerList[rightAnswerId - 1];
        }
    }
    #endregion

    #region 3. Exam Base and Derived Classes
    public abstract class Exam : ICloneable
    {
        public int Time { get; set; }
        public int NumberOfQuestions { get; set; }
        public Question[] Questions { get; set; } = Array.Empty<Question>();

        public Exam() { }

        public Exam(int time, int numberOfQuestions)
        {
            Time = time;
            NumberOfQuestions = numberOfQuestions;
        }

        public abstract void CreateExam();
        public abstract void ShowExam();

        public object Clone()
        {
            Exam cloned = (Exam)this.MemberwiseClone();
            if (Questions != null)
            {
                cloned.Questions = new Question[Questions.Length];
                for (int i = 0; i < Questions.Length; i++)
                {
                    cloned.Questions[i] = (Question)Questions[i].Clone();
                }
            }
            return cloned;
        }

        public override string ToString()
        {
            return $"Exam Time: {Time} mins, Total Questions: {NumberOfQuestions}";
        }
    }

    public class FinalExam : Exam
    {
        public FinalExam() { }

        public FinalExam(int time, int numberOfQuestions) 
            : base(time, numberOfQuestions)
        {
        }

        public override void CreateExam()
        {
            Questions = new Question[NumberOfQuestions];

            for (int i = 0; i < NumberOfQuestions; i++)
            {
                Console.Clear();
                int qType;
                do
                {
                    Console.Write($"Please Choose Question Type for Question ({i + 1}) [1 for True/False, 2 for MCQ]: ");
                } while (!int.TryParse(Console.ReadLine(), out qType) || (qType != 1 && qType != 2));

                Console.Clear();
                string header = qType == 1 ? "True/False Question" : "MCQ Question";
                Console.WriteLine($"{header}");

                Console.Write("Please Enter Question Body: ");
                string body = Console.ReadLine() ?? string.Empty;

                double mark;
                do
                {
                    Console.Write("Please Enter Question Mark: ");
                } while (!double.TryParse(Console.ReadLine(), out mark) || mark <= 0);

                if (qType == 1)
                    Questions[i] = new TFQuestion(header, body, mark);
                else
                    Questions[i] = new McqQuestion(header, body, mark);

                Questions[i].CreateQuestion();
            }
        }

        public override void ShowExam()
        {
            double totalMarks = 0;
            double obtainedMarks = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < Questions.Length; i++)
            {
                Console.Clear();
                Console.WriteLine($"Question ({i + 1}) of ({Questions.Length})");
                Console.WriteLine(Questions[i]);

                for (int j = 0; j < Questions[i].AnswerList.Length; j++)
                {
                    Console.WriteLine(Questions[i].AnswerList[j]);
                }

                int userAnswerId;
                do
                {
                    Console.Write("\nYour Answer (Enter Answer Id): ");
                } while (!int.TryParse(Console.ReadLine(), out userAnswerId) || userAnswerId < 1 || userAnswerId > Questions[i].AnswerList.Length);

                Questions[i].UserAnswer = Questions[i].AnswerList[userAnswerId - 1];
                totalMarks += Questions[i].Mark;

                if (Questions[i].UserAnswer.AnswerId == Questions[i].RightAnswer.AnswerId)
                {
                    obtainedMarks += Questions[i].Mark;
                }
            }

            sw.Stop();
            Console.Clear();

            Console.WriteLine("=================== Exam Result ===================\n");
            for (int i = 0; i < Questions.Length; i++)
            {
                Console.WriteLine($"Q{i + 1}) {Questions[i].Body}");
                Console.WriteLine($"   Your Answer: {Questions[i].UserAnswer.AnswerText}");
                Console.WriteLine($"   Right Answer: {Questions[i].RightAnswer.AnswerText}\n");
            }

            Console.WriteLine($"Your Grade: {obtainedMarks} from {totalMarks}");
            Console.WriteLine($"Time Elapsed: {sw.Elapsed}");
            Console.WriteLine("===================================================");
        }
    }

    public class PracticalExam : Exam
    {
        public PracticalExam() { }

        public PracticalExam(int time, int numberOfQuestions) 
            : base(time, numberOfQuestions)
        {
        }

        public override void CreateExam()
        {
            Questions = new Question[NumberOfQuestions];

            for (int i = 0; i < NumberOfQuestions; i++)
            {
                Console.Clear();
                Console.WriteLine($"MCQ Question ({i + 1})");

                Console.Write("Please Enter Question Body: ");
                string body = Console.ReadLine() ?? string.Empty;

                double mark;
                do
                {
                    Console.Write("Please Enter Question Mark: ");
                } while (!double.TryParse(Console.ReadLine(), out mark) || mark <= 0);

                Questions[i] = new McqQuestion("MCQ Question", body, mark);
                Questions[i].CreateQuestion();
            }
        }

        public override void ShowExam()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < Questions.Length; i++)
            {
                Console.Clear();
                Console.WriteLine($"Question ({i + 1}) of ({Questions.Length})");
                Console.WriteLine(Questions[i]);

                for (int j = 0; j < Questions[i].AnswerList.Length; j++)
                {
                    Console.WriteLine(Questions[i].AnswerList[j]);
                }

                int userAnswerId;
                do
                {
                    Console.Write("\nYour Answer (Enter Answer Id): ");
                } while (!int.TryParse(Console.ReadLine(), out userAnswerId) || userAnswerId < 1 || userAnswerId > Questions[i].AnswerList.Length);

                Questions[i].UserAnswer = Questions[i].AnswerList[userAnswerId - 1];
            }

            sw.Stop();
            Console.Clear();

            Console.WriteLine("=================== Right Answers ===================\n");
            for (int i = 0; i < Questions.Length; i++)
            {
                Console.WriteLine($"Q{i + 1}) {Questions[i].Body}");
                Console.WriteLine($"   Right Answer: {Questions[i].RightAnswer.AnswerText}\n");
            }

            Console.WriteLine($"Time Elapsed: {sw.Elapsed}");
            Console.WriteLine("====================================================");
        }
    }
    #endregion

    #region 4. Subject Class
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public Exam? SubjectExam { get; set; }

        public Subject(int subjectId, string subjectName)
        {
            SubjectId = subjectId;
            SubjectName = subjectName ?? string.Empty;
        }

        public void CreateExam()
        {
            int examType;
            do
            {
                Console.Write("Please Enter Type Of Exam (1 for Practical, 2 for Final): ");
            } while (!int.TryParse(Console.ReadLine(), out examType) || (examType != 1 && examType != 2));

            int time;
            do
            {
                Console.Write("Please Enter Time For Exam In Minutes: ");
            } while (!int.TryParse(Console.ReadLine(), out time) || time <= 0);

            int numberOfQuestions;
            do
            {
                Console.Write("Please Enter Number Of Questions: ");
            } while (!int.TryParse(Console.ReadLine(), out numberOfQuestions) || numberOfQuestions <= 0);

            if (examType == 1)
                SubjectExam = new PracticalExam(time, numberOfQuestions);
            else
                SubjectExam = new FinalExam(time, numberOfQuestions);

            SubjectExam.CreateExam();
        }

        public override string ToString()
        {
            return $"Subject ID: {SubjectId}, Name: {SubjectName}";
        }
    }
    #endregion

    #region 5. Program Entry Point
    class Program
    {
        static void Main(string[] args)
        {
            Subject sub1 = new Subject(101, "Object Oriented Programming (OOP)");

            Console.WriteLine($"Welcome to Subject: {sub1.SubjectName}");
            Console.WriteLine("--------------------------------------------");

            sub1.CreateExam();

            Console.Clear();
            char startChoice;
            do
            {
                Console.Write("Do You Want To Start The Exam? (Y|N): ");
            } while (!char.TryParse(Console.ReadLine()?.ToUpper(), out startChoice) || (startChoice != 'Y' && startChoice != 'N'));

            if (startChoice == 'Y')
            {
                Console.Clear();
                sub1.SubjectExam?.ShowExam();
            }
            else
            {
                Console.WriteLine("Exam Cancelled. Good Luck!");
            }
        }
    }
    #endregion
}
