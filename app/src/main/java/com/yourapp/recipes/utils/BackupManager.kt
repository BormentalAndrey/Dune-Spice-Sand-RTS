package com.yourapp.recipes.utils

import android.content.Context
import com.google.gson.Gson
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.domain.model.Recipe
import kotlinx.coroutines.flow.first
import java.io.File
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class BackupManager @Inject constructor(
    private val context: Context,
    private val recipeDao: RecipeDao,
    private val gson: Gson
) {
    
    suspend fun exportToJson(): String {
        val recipes = recipeDao.getFilteredRecipes().first()
        return gson.toJson(mapOf(
            "version" to 1,
            "exportDate" to System.currentTimeMillis(),
            "recipes" to recipes
        ))
    }
    
    fun saveBackupToFile(json: String): File {
        val dateFormat = SimpleDateFormat("yyyy-MM-dd_HH-mm", Locale.getDefault())
        val fileName = "recipes_backup_${dateFormat.format(Date())}.json"
        val backupDir = File(context.getExternalFilesDir(null), "backups")
        
        if (!backupDir.exists()) {
            backupDir.mkdirs()
        }
        
        val backupFile = File(backupDir, fileName)
        backupFile.writeText(json)
        return backupFile
    }
    
    suspend fun importFromJson(json: String): Int {
        val data = gson.fromJson(json, Map::class.java)
        val recipesJson = data["recipes"] as? List<Map<String, Any>> ?: return 0
        
        var importedCount = 0
        recipesJson.forEach { recipeMap ->
            try {
                val recipe = gson.fromJson(
                    gson.toJson(recipeMap),
                    Recipe::class.java
                )
                recipeDao.insertRecipe(recipe.toEntity())
                importedCount++
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
        
        return importedCount
    }
}
