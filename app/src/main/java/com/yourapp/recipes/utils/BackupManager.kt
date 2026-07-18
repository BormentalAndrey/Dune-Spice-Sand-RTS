package com.yourapp.recipes.utils

import android.content.Context
import com.google.gson.Gson
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.domain.model.Ingredient
import com.yourapp.recipes.domain.model.CookingStep
import com.yourapp.recipes.domain.model.Recipe
import com.yourapp.recipes.domain.model.NutritionInfo
import com.yourapp.recipes.domain.model.Difficulty
import com.yourapp.recipes.domain.model.Category
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
                val recipe = mapToRecipe(recipeMap)
                val entity = recipeToEntity(recipe)
                recipeDao.insertRecipe(entity)
                importedCount++
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
        
        return importedCount
    }
    
    private fun mapToRecipe(map: Map<String, Any>): Recipe {
        return Recipe(
            title = map["title"] as? String ?: "",
            description = map["description"] as? String ?: "",
            cookingTimeMinutes = (map["cookingTimeMinutes"] as? Double)?.toInt() ?: 0,
            difficulty = try {
                Difficulty.valueOf(map["difficulty"] as? String ?: "EASY")
            } catch (e: Exception) {
                Difficulty.EASY
            },
            category = try {
                Category.valueOf(map["category"] as? String ?: "DINNER")
            } catch (e: Exception) {
                Category.DINNER
            },
            servings = (map["servings"] as? Double)?.toInt() ?: 2,
            photoPath = map["photoPath"] as? String,
            isFavorite = map["isFavorite"] as? Boolean ?: false
        )
    }
    
    private fun recipeToEntity(recipe: Recipe): RecipeEntity {
        return RecipeEntity(
            title = recipe.title,
            description = recipe.description,
            cookingTimeMinutes = recipe.cookingTimeMinutes,
            difficulty = recipe.difficulty.name,
            category = recipe.category.name,
            ingredientsJson = gson.toJson(recipe.ingredients),
            stepsJson = gson.toJson(recipe.steps),
            calories = recipe.nutritionInfo.calories,
            proteins = recipe.nutritionInfo.proteins,
            fats = recipe.nutritionInfo.fats,
            carbohydrates = recipe.nutritionInfo.carbohydrates,
            servings = recipe.servings,
            photoPath = recipe.photoPath,
            isFavorite = recipe.isFavorite
        )
    }
}
