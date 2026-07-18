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
        val calendar = Calendar.getInstance().apply {
            timeInMillis = startDate
            set(Calendar.DAY_OF_WEEK, Calendar.MONDAY)
            set(Calendar.HOUR_OF_DAY, 0)
            set(Calendar.MINUTE, 0)
            set(Calendar.SECOND, 0)
            set(Calendar.MILLISECOND, 0)
        }
        
        val endDate = calendar.timeInMillis + 7 * 24 * 60 * 60 * 1000
        
        return mealPlanDao.getMealPlansForPeriod(calendar.timeInMillis, endDate).map { list ->
            list.map { it.mealPlan.toDomain(it.recipe.toDomain()) }
                .groupBy { it.date }
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
        // Implementation for drag-and-drop
        val mealPlan = mealPlanDao.getMealPlansForDate(newDate)
        // Update logic here
    }
    
    private fun MealPlanEntity.toDomain(recipe: Recipe? = null): MealPlan {
        return MealPlan(
            id = id,
            recipeId = recipeId,
            date = date,
            mealType = MealType.valueOf(mealType.uppercase()),
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
}
