﻿using NUnit.Framework;
using ScottPlot.Cookbook.Site;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlotTests.Cookbook
{
    class Validate
    {
        const string SourceFolder = "../../../../cookbook/ScottPlot.Cookbook";

        [Test]
        public void Test_CookbookRecipes_Exist()
        {
            var recipes = ScottPlot.Cookbook.Locate.GetRecipes();
            Assert.IsNotEmpty(recipes);

            foreach (var recipe in recipes)
                Console.WriteLine(recipe);
        }

        [Test]
        public void Test_CookbookRecipes_AllHaveSourceCode()
        {
            var recipes = ScottPlot.Cookbook.Locate.GetRecipes();
            var chef = new ScottPlot.Cookbook.Chef();
            var sources = chef.GetRecipeSources(SourceFolder);

            foreach (ScottPlot.Cookbook.IRecipe recipe in recipes)
                Assert.AreEqual(1, sources.Where(x => x.id == recipe.ID).Count());
        }

        [Test]
        public void Test_CookbookRecipes_DescriptorsAreValid()
        {
            string allowedSpecialCharacters = " _'()[];.,/";
            foreach (var recipe in ScottPlot.Cookbook.Locate.GetRecipes())
            {
                foreach (var c in recipe.Title + recipe.Description)
                {
                    if (char.IsLetterOrDigit(c) || allowedSpecialCharacters.Contains(c))
                        continue;
                    else
                        Assert.Fail($"{recipe.ID} Title or Description contains a special character: {c}");
                }
            }
        }

        [Test]
        public void Test_CookbookRecipes_IDsAreValid()
        {
            string allowedSpecialCharacters = "_";
            foreach (var recipe in ScottPlot.Cookbook.Locate.GetRecipes())
            {
                foreach (var c in recipe.ID.ToCharArray())
                {
                    if (char.IsLetterOrDigit(c) || allowedSpecialCharacters.Contains(c))
                        continue;
                    else
                        Assert.Fail("IDs must not contain special characters (except underscore)");
                }
            }
        }

        [Test]
        public void Test_CookbookRecipes_IDsAreUnique()
        {
            var recipes = ScottPlot.Cookbook.Locate.GetRecipes();
            string[] ids = recipes.Select(x => x.ID.ToLower()).ToArray();

            if (ids.Distinct().Count() < ids.Length)
                Assert.Fail("all lowercase recipe IDs must be unique");
        }

        [Test]
        public void Test_CookbookRecipes_CategoriesRemainUniqueAfterSanitization()
        {
            var recipes = ScottPlot.Cookbook.Locate.GetRecipes();
            int uniqueFull = recipes.Select(x => x.Category).Distinct().Count();
            int uniqueClean = recipes.Select(x => x.Category).Select(x => SiteGenerator.Sanitize(x)).Distinct().Count();
            Assert.AreEqual(uniqueClean, uniqueFull);
        }
    }
}
