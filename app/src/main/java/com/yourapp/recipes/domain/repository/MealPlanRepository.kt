package com.yourapp.recipes.domain.repository

import com.yourapp.recipes.domain.model.MealPlan
import kotlinx.coroutines.flow.Flow

interface MealPlanRepository {
    fun getMealPlansForDate(date: Long): Flow<List<MealPlan>>
    fun getMealPlansForWeek(startDate: Long): Flow<Map<Long, List<MealPlan>>>
    suspend fun addMealPlan(mealPlan: MealPlan)
    suspend fun updateMealPlan(mealPlan: MealPlan)
    suspend fun deleteMealPlan(mealPlanId: Long)
    suspend fun moveMealPlan(mealPlanId: Long, newDate: Long)
}
