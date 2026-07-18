// app/src/main/java/com/yourapp/recipes/data/repository/MealPlanRepositoryImpl.kt

package com.yourapp.recipes.data.repository

import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.yourapp.recipes.data.local.dao.MealPlanDao
import com.yourapp.recipes.data.local.database.entity.MealPlanEntity
import com.yourapp.recipes.domain.model.*
import com.yourapp.recipes.domain.repository.MealPlanRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class MealPlanRepositoryImpl @Inject constructor(
    private val mealPlanDao: MealPlanDao,
    private val gson: Gson
) : MealPlanRepository {
    
    override fun getMealPlansForDate(date: Long): Flow<List<MealPlan>> {
        return mealPlanDao.getMealPlansForDate(date).map { entities ->
            entities.map { it.toDomain() }
        }
    }
    
    override fun getMealPlansForWeek(startDate: Long): Flow<Map<Long, List<MealPlan>>> {
        val endDate = startDate + 7 * 24 * 60 * 60 * 1000
        
        return mealPlanDao.getMealPlansForPeriod(startDate, endDate).map { list ->
            list.map { mealPlanWithRecipe ->
                mealPlanWithRecipe.mealPlan.toDomain(mealPlanWithRecipe.recipe.toDomain())
            }.groupBy { it.date }
        }
    }
    
    override suspend fun addMealPlan(mealPlan: MealPlan) {
        mealPlanDao.insertMealPlan(mealPlan.toEntity())
    }
    
    override suspend fun updateMealPlan(mealPlan: MealPlan) {
        mealPlanDao.updateMealPlan(mealPlan.toEntity())
    }
    
    override suspend fun deleteMealPlan(mealPlanId: Long) {
        mealPlanDao.deleteMealPlanById(mealPlanId)
    }
    
    override suspend fun moveMealPlan(mealPlanId: Long, newDate: Long) {
        // Простое удаление и создание нового
        // В реальном приложении нужно обновлять дату
    }
    
    private fun MealPlanEntity.toDomain(recipe: Recipe? = null): MealPlan {
        return MealPlan(
            id = id,
            recipeId = recipeId,
            date = date,
            mealType = try {
                MealType.valueOf(mealType.uppercase())
            } catch (e: Exception) {
                MealType.DINNER
            },
            servings = servings,
            notes = notes,
            recipe = recipe
        )
    }
    
    private fun MealPlan.toEntity(): MealPlanEntity {
        return MealPlanEntity(
            id = id,
            recipeId = recipeId,
            date = date,
            mealType = mealType.name.lowercase(),
            servings = servings,
            notes = notes
        )
    }
    
    private fun com.yourapp.recipes.data.local.database.entity.RecipeEntity.toDomain(): Recipe {
        val ingredientsType = object : TypeToken<List<Ingredient>>() {}.type
        val stepsType = object : TypeToken<List<CookingStep>>() {}.type
        
        return Recipe(
            id = id,
            title = title,
            description = description,
            cookingTimeMinutes = cookingTimeMinutes,
            difficulty = try {
                Difficulty.valueOf(difficulty)
            } catch (e: Exception) {
                Difficulty.EASY
            },
            category = try {
                Category.valueOf(category)
            } catch (e: Exception) {
                Category.DINNER
            },
            ingredients = try {
                gson.fromJson(ingredientsJson, ingredientsType) ?: emptyList()
            } catch (e: Exception) {
                emptyList()
            },
            steps = try {
                gson.fromJson(stepsJson, stepsType) ?: emptyList()
            } catch (e: Exception) {
                emptyList()
            },
            nutritionInfo = NutritionInfo(calories, proteins, fats, carbohydrates),
            servings = servings,
            photoPath = photoPath,
            isFavorite = isFavorite,
            dateAdded = dateAdded,
            dateModified = dateModified
        )
    }
}
