using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sol_Job_Bank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace sol_Job_Bank.Data
{
    public static class JBSeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new JobBankContext(
                serviceProvider.GetRequiredService<DbContextOptions<JobBankContext>>()))
            {
                //Prepare Random
                Random random = new Random();

                //Seed Occupations
                string[] occupations = new string[] { "Teacher", "Nurse", "Building Contractor", "Electrician", "Lawyer", "Doctor" };
                if (!context.Occupations.Any())
                {
                    foreach (string s in occupations)
                    {
                        Occupation o = new Occupation
                        {
                            Title = s
                        };
                        context.Occupations.Add(o);
                    }
                    context.SaveChanges();
                }
                //Create collection of the primary keys of the Occupations
                int[] OccupationIDs = context.Occupations.Select(s => s.ID).ToArray();
                int occupationIDCount = OccupationIDs.Count();

                //Create 5 notes from Bacon ipsum
                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                //Seed Positions
                string[] positions = new string[] { "Emergency Room Nurse", "Kindergarten Teacher", "Marine Electrician", "Divorce Lawyer", "Obstetrician", "Gofer", "Lackey" };
                if (!context.Positions.Any())
                {
                    foreach (string s in positions)
                    {
                        Position p = new Position
                        {
                            Name = s,
                            Description = baconNotes[random.Next(5)],
                            Salary = random.Next(240000, 29800000) / 100m,
                            OccupationID = OccupationIDs[random.Next(occupationIDCount)]
                        };
                        context.Positions.Add(p);
                    }
                    context.SaveChanges();
                }

                //Create collection of the primary keys of the Positions
                int[] PositionIDs = context.Positions.Select(s => s.ID).ToArray();
                int positionIDCount = PositionIDs.Count();

                //Seed Postings
                //Add a StartDate to every second Posting
                if (!context.Postings.Any())
                {
                    int toggle = 0; //Used to alternate
                    for (int i = 0; i < positionIDCount; i++)
                    {
                        Posting p = new Posting()
                        {
                            NumberOpen = random.Next(11),
                            ClosingDate = DateTime.Today.AddDays(random.Next(60)),
                            PositionID = PositionIDs[i]
                        };
                        toggle++;
                        if (toggle % 2 == 0)//Every second Posting gets a StartDate
                        {
                            p.StartDate = p.ClosingDate.AddDays(random.Next(30));
                        }
                        context.Postings.Add(p);
                    }
                    context.SaveChanges();
                }

                //Seed Programs
                string[] programs = new string[] { "First Aid,", "CPR",
                    "Computer Programming,", "Life Skills", "Carpentry" };
                if (!context.RetrainingPrograms.Any())
                {
                    foreach (string s in programs)
                    {
                        RetrainingProgram p = new RetrainingProgram
                        {
                            Name = s
                        };
                        context.RetrainingPrograms.Add(p);
                    }
                    context.SaveChanges();
                }

                //Seed Skills
                string[] skills = new string[] { "Communications Skills", "Organizational Skills", 
                    "Writing", "Customer Service", "Microsoft Excel", "Problem Solving",
                    "Planning", "Microsoft Office", "Research", "Detail Oriented", 
                    "Project Management", "Building Effective Relationships", "Computer Skills",
                    "QA and Control", "Troubleshooting" };
                if (!context.Skills.Any())
                {
                    foreach (string s in skills)
                    {
                        Skill o = new Skill
                        {
                            Name = s
                        };
                        context.Skills.Add(o);
                    }
                    context.SaveChanges();
                }
                //Create collection of the primary keys of the Skills
                int[] SkillIDs = context.Skills.Select(s => s.ID).ToArray();
                int skillIDCount = SkillIDs.Count();

                //Seed Applicants
                string[] firstNames = new string[] { "Woodstock", "Sally", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder", "Fred", "Barney", "Wilma", "Betty" };
                string[] lastNames = new string[] { "Hightower", "Wizard", "Kingfisher", "Prometheus", "Broomspun", "Shooter", "Chuckles", "Stovell", "Jones", "Bloggs", "Flintstone", "Rubble", "Brown", "Smith", "Daniel" };
                if (!context.Applicants.Any())
                {
                    List<Applicant> applicants = new List<Applicant>();
                    foreach (string lastName in lastNames)
                    {
                        foreach (string firstname in firstNames)
                        {
                            //Construct some Applicant details
                            Applicant a = new Applicant()
                            {
                                FirstName = firstname,
                                LastName = lastName,
                                MiddleName = lastName[1].ToString().ToUpper(),
                                SIN = random.Next(213214131, 989898989).ToString(),
                                eMail = (firstname.Substring(0, 2) + lastName + random.Next(11, 111).ToString() + "@outlook.com").ToLower(),
                                Phone = Convert.ToInt64(random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString())
                            };
                            context.Applicants.Add(a);
                            try
                            {
                                //Could be a duplicate email
                                context.SaveChanges();
                            }
                            catch (Exception)
                            {
                                //so skip it and go on to the next
                            }
                        }
                    }
                    
                }

                //Create collection of the primary keys of the Postings
                int[] PostingIDs = context.Postings.Select(s => s.ID).ToArray();
                int postingIDCount = PostingIDs.Count();

                //Create collection of the primary keys of the Applicants
                int[] ApplicantIDs = context.Applicants.Select(s => s.ID).ToArray();
                int applicantIDCount = ApplicantIDs.Count();

                //Seed Applications - The Loaded Intersection
                //Have every second Applicant applly to one posting
                if (!context.Applications.Any())
                {
                    for (int i = 0; i < applicantIDCount; i++)
                    {
                        if (i % 2 == 0)//Every second Applicant 
                        {
                            Application a = new Application()
                            {
                                ApplicantID = ApplicantIDs[i],
                                PostingID = PostingIDs[random.Next(postingIDCount)],
                                Comments= baconNotes[random.Next(5)]
                            };
                            context.Applications.Add(a);
                        }
                    }
                    context.SaveChanges();
                }

                //Seed Skills for Applicants - The No Load Intersection
                //Have every second Applicant get a couple of skills
                if (!context.ApplicantSkills.Any())
                {
                    for (int i = 0; i < applicantIDCount; i++)
                    {
                        if (i % 2 == 0)//Every second Applicant 
                        {
                            //We will add 2 skills
                            int skillID = random.Next(skillIDCount - 1);
                            ApplicantSkill a = new ApplicantSkill()
                            {
                                ApplicantID = ApplicantIDs[i],
                                SkillID = SkillIDs[skillID]
                            };
                            context.ApplicantSkills.Add(a);
                            ApplicantSkill a1 = new ApplicantSkill()
                            {
                                ApplicantID = ApplicantIDs[i],
                                SkillID = SkillIDs[skillID + 1]
                            };
                            context.ApplicantSkills.Add(a1);
                        }
                    }
                    context.SaveChanges();
                }
                //Project Part 3A Seed Skills for Positions - The No Load Intersection
                //Have every second Position get a couple of skills
                if (!context.PositionSkills.Any())
                {
                    for (int i = 0; i < positionIDCount; i++)
                    {
                        if (i % 2 == 0)//Every second Position 
                        {
                            //We will add 2 skills
                            int skillID = random.Next(skillIDCount - 1);
                            PositionSkill a = new PositionSkill()
                            {
                                PositionID = PositionIDs[i],
                                SkillID = SkillIDs[skillID]
                            };
                            context.PositionSkills.Add(a);
                            PositionSkill a1 = new PositionSkill()
                            {
                                PositionID = PositionIDs[i],
                                SkillID = SkillIDs[skillID + 1]
                            };
                            context.PositionSkills.Add(a1);
                        }
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
