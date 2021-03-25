﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TDD.DbTestHelpers.EF;
using YamlDotNet.RepresentationModel.Serialization;

namespace TDD.DbTestHelpers.Helpers
{
    public class FileHelper
    {
        public void ClearTables<TFixtureType>(DbContext context)
        {
            ClearTables(typeof(TFixtureType), context);
        }

        public void ClearTables(Type fixtureType, DbContext context)
        {
            foreach (var fixtureTable in fixtureType.GetProperties())
            {
                var table = context.GetType().GetProperty(fixtureTable.Name);
                var tableType = table.PropertyType;
                var clearTableMethod = typeof (EfExtensions).GetMethod("ClearTable")
                    .MakeGenericMethod(tableType.GetGenericArguments());
                clearTableMethod.Invoke(null, new[] {table.GetValue(context, null)});
            }
            context.SaveChanges();
        }
        
        public void FillFixturesFileFiles<TFixtureType>(DbContext context, string yamlFolderName, IEnumerable<string> yamlFilesNames)
        {
            FillFixturesFileFiles(typeof(TFixtureType), context, yamlFolderName, yamlFilesNames);    
        }

        public void FillFixturesFileFiles(Type fixtureType, DbContext context, string yamlFolderName, IEnumerable<string> yamlFullFilesNames)
        {
            var fixtures = GetFixutresFromYaml(fixtureType, yamlFolderName, yamlFullFilesNames);
            foreach (var fixtureTable in fixtures.GetType().GetProperties())
            {
                var table = fixtureTable.GetValue(fixtures, null) as IDictionary;
                if (table == null) throw new Exception("Cannot read entities from table " + fixtureTable.Name);
                foreach (var entity in table.Values)
                {
                    var dbSetType = context.GetType().GetProperty(fixtureTable.Name);
                    if (dbSetType == null)
                        throw new Exception(string.Format("Cannot find table {0} in database", fixtureTable.Name));
                    var dbSet = dbSetType.GetValue(context, null);
                    var makeGenericType = typeof(DbSet<>).MakeGenericType(entity.GetType());
                    var methodInfo = makeGenericType.GetMethod("Add");
                    methodInfo.Invoke(dbSet, new[] { entity });
                }
            }
            context.SaveChanges();
        }

        private object GetFixutresFromYaml(Type fixtureType, string yamlFolderName, IEnumerable<string> yamlFullFilesNames)
        {
            Type type = typeof(YamlSerializer<>).MakeGenericType(fixtureType);
            object yamlSerializer = Activator.CreateInstance(type);

            var methodInfo = yamlSerializer.GetType().GetMethod("Deserialize", new [] { typeof(TextReader )});
            try
            {
                return methodInfo.Invoke(yamlSerializer, new [] { GetAllYamlConfiguration(yamlFullFilesNames, yamlFolderName) });
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize YAML file. See inner exception.", ex);
            }
        }


        private static TextReader GetAllYamlConfiguration(IEnumerable<string> yamlFilesNames, string yamlFolderName)
        {
            var sb = new StringBuilder();
            foreach (var yamlFileName in yamlFilesNames)
            {
                var yamlPath = Path.Combine(yamlFolderName, yamlFileName);
                if (!File.Exists(yamlPath))
                    throw new Exception(String.Format("Specified file {0} does not exist in specifiled folder {1}",
                                                      yamlFileName, yamlFolderName));
                sb.AppendLine(File.ReadAllText(yamlPath));
            }
            return new StringReader(sb.ToString());
        }
    }
}
