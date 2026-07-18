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

/**
 * Реализация репозитория рецептов.
 * Отвечает за преобразование между сущностями БД и доменными моделями.
 */
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
        // Сохраняем ингредиенты для автодополнения
        recipe.ingredients.forEach { ingredient ->
            ingredientDao.insertOrUpdateIngredient(
                IngredientEntity(name = ingredient.name, category = ingredient.category)
            )
            ingredientDao.incrementUsageCount(ingredient.name)
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
    
    // Extension functions for mapping
    private fun RecipeEntity.toDomain(): Recipe {
        val ingredientsType = object : TypeToken<List<Ingredient>>() {}.type
        val stepsType = object : TypeToken<List<CookingStep>>() {}.type
        
        return Recipe(
            id = id,
            title = title,
            description = description,
            cookingTimeMinutes = cookingTimeMinutes,
            difficulty = Difficulty.valueOf(difficulty),
            category = Category.valueOf(category),
            ingredients = gson.fromJson(ingredientsJson, ingredientsType) ?: emptyList(),
            steps = gson.fromJson(stepsJson, stepsType) ?: emptyList(),
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
