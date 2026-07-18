package com.yourapp.recipes.data.repository

import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.yourapp.recipes.data.local.dao.IngredientDao
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.data.local.database.entity.IngredientEntity
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import com.yourapp.recipes.domain.model.*
import com.yourapp.recipes.domain.repository.RecipeRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class RecipeRepositoryImpl @Inject constructor(
    private val recipeDao: RecipeDao,
    private val ingredientDao: IngredientDao,
    private val gson: Gson
) : RecipeRepository {
    
    override fun getFilteredRecipes(filter: RecipeFilter): Flow<List<Recipe>> {
        return recipeDao.getFilteredRecipes(
            searchQuery = filter.searchQuery,
            category = filter.category?.name,
            difficulty = filter.difficulty?.name,
            maxCookingTime = filter.maxCookingTime,
            onlyFavorites = filter.onlyFavorites,
            sortBy = filter.sortBy.name
        ).map { entities ->
            entities.map { it.toDomain() }
        }
    }
    
    override suspend fun getRecipeById(recipeId: Long): Recipe? {
        return recipeDao.getRecipeById(recipeId)?.toDomain()
    }
    
    override fun getRecipeByIdFlow(recipeId: Long): Flow<Recipe?> {
        return recipeDao.getRecipeByIdFlow(recipeId).map { it?.toDomain() }
    }
    
    override suspend fun getRandomRecipe(): Recipe? {
        return recipeDao.getRandomRecipe()?.toDomain()
    }
    
    override fun getFavoriteRecipes(): Flow<List<Recipe>> {
        return recipeDao.getFavoriteRecipes().map { entities ->
            entities.map { it.toDomain() }
        }
    }
    
    override suspend fun saveRecipe(recipe: Recipe): Long {
        val entity = recipe.toEntity()
        recipe.ingredients.forEach { ingredient ->
            try {
                ingredientDao.insertOrUpdateIngredient(
                    IngredientEntity(name = ingredient.name, category = ingredient.category)
                )
                ingredientDao.incrementUsageCount(ingredient.name)
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
        return if (recipe.id == 0L) {
            recipeDao.insertRecipe(entity)
        } else {
            recipeDao.updateRecipe(entity)
            recipe.id
        }
    }
    
    override suspend fun deleteRecipe(recipeId: Long) {
        recipeDao.deleteRecipe(recipeId)
    }
    
    override suspend fun updateFavoriteStatus(recipeId: Long, isFavorite: Boolean) {
        recipeDao.updateFavoriteStatus(recipeId, isFavorite)
    }
    
    override fun searchIngredients(query: String): Flow<List<String>> {
        return ingredientDao.searchIngredients(query).map { entities ->
            entities.map { it.name }
        }
    }
    
    override suspend fun getRecipeCount(): Int {
        return recipeDao.getRecipeCount()
    }
    
    private fun RecipeEntity.toDomain(): Recipe {
        val ingredients = try {
            val listType = object : TypeToken<List<Map<String, Any>>>() {}.type
            val rawList: List<Map<String, Any>> = gson.fromJson(ingredientsJson, listType) ?: emptyList()
            rawList.map { map ->
                Ingredient(
                    id = (map["id"] as? String) ?: java.util.UUID.randomUUID().toString(),
                    name = (map["name"] as? String) ?: "",
                    quantity = ((map["quantity"] as? Number)?.toFloat()) ?: 0f,
                    unit = (map["unit"] as? String) ?: "",
                    category = (map["category"] as? String) ?: "other"
                )
            }
        } catch (e: Exception) {
            emptyList()
        }
        
        val steps = try {
            val listType = object : TypeToken<List<Map<String, Any>>>() {}.type
            val rawList: List<Map<String, Any>> = gson.fromJson(stepsJson, listType) ?: emptyList()
            rawList.map { map ->
                CookingStep(
                    stepNumber = ((map["stepNumber"] as? Number)?.toInt()) ?: 0,
                    description = (map["description"] as? String) ?: "",
                    durationMinutes = ((map["durationMinutes"] as? Number)?.toInt()) ?: 0,
                    isCompleted = (map["isCompleted"] as? Boolean) ?: false
                )
            }
        } catch (e: Exception) {
            emptyList()
        }
        
        return Recipe(
            id = id,
            title = title,
            description = description,
            cookingTimeMinutes = cookingTimeMinutes,
            difficulty = try { Difficulty.valueOf(difficulty) } catch (e: Exception) { Difficulty.EASY },
            category = try { Category.valueOf(category) } catch (e: Exception) { Category.DINNER },
            ingredients = ingredients,
            steps = steps,
            nutritionInfo = NutritionInfo(calories, proteins, fats, carbohydrates),
            servings = servings,
            photoPath = photoPath,
            isFavorite = isFavorite,
            dateAdded = dateAdded,
            dateModified = dateModified
        )
    }
    
    private fun Recipe.toEntity(): RecipeEntity {
        return RecipeEntity(
            id = id,
            title = title,
            description = description,
            cookingTimeMinutes = cookingTimeMinutes,
            difficulty = difficulty.name,
            category = category.name,
            ingredientsJson = gson.toJson(ingredients),
            stepsJson = gson.toJson(steps),
            calories = nutritionInfo.calories,
            proteins = nutritionInfo.proteins,
            fats = nutritionInfo.fats,
            carbohydrates = nutritionInfo.carbohydrates,
            servings = servings,
            photoPath = photoPath,
            isFavorite = isFavorite,
            dateAdded = dateAdded,
            dateModified = System.currentTimeMillis()
        )
    }
}
