package com.yourapp.recipes.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.yourapp.recipes.domain.model.MealPlan
import com.yourapp.recipes.domain.model.MealType
import com.yourapp.recipes.domain.model.Recipe
import com.yourapp.recipes.domain.model.RecipeFilter
import com.yourapp.recipes.domain.repository.MealPlanRepository
import com.yourapp.recipes.domain.repository.RecipeRepository
import com.yourapp.recipes.domain.usecase.shopping.GenerateShoppingListUseCase
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import java.util.*
import javax.inject.Inject

@HiltViewModel
class MealPlannerViewModel @Inject constructor(
    private val mealPlanRepository: MealPlanRepository,
    private val recipeRepository: RecipeRepository,
    private val generateShoppingListUseCase: GenerateShoppingListUseCase
) : ViewModel() {
    
    private val _currentWeekStart = MutableStateFlow(getStartOfWeek())
    val currentWeekStart: StateFlow<Long> = _currentWeekStart.asStateFlow()
    
    val weekMealPlans = _currentWeekStart.flatMapLatest { startDate ->
        mealPlanRepository.getMealPlansForWeek(startDate)
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyMap())
    
    private val _selectedDate = MutableStateFlow(getTodayDate())
    val selectedDate: StateFlow<Long> = _selectedDate.asStateFlow()
    
    val dayMealPlans = _selectedDate.flatMapLatest { date ->
        mealPlanRepository.getMealPlansForDate(date)
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyList())
    
    val allRecipes: StateFlow<List<Recipe>> = recipeRepository.getFilteredRecipes(RecipeFilter())
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyList())
    
    fun selectDate(date: Long) {
        _selectedDate.value = date
    }
    
    fun previousWeek() {
        _currentWeekStart.value -= 7 * 24 * 60 * 60 * 1000
    }
    
    fun nextWeek() {
        _currentWeekStart.value += 7 * 24 * 60 * 60 * 1000
    }
    
    fun addMealToPlan(recipeId: Long, date: Long, mealType: MealType) {
        viewModelScope.launch {
            mealPlanRepository.addMealPlan(
                MealPlan(
                    recipeId = recipeId,
                    date = date,
                    mealType = mealType
                )
            )
        }
    }
    
    fun removeMealFromPlan(mealPlanId: Long) {
        viewModelScope.launch {
            mealPlanRepository.deleteMealPlan(mealPlanId)
        }
    }
    
    fun generateShoppingList() {
        viewModelScope.launch {
            val startDate = _currentWeekStart.value
            val endDate = startDate + 7 * 24 * 60 * 60 * 1000
            generateShoppingListUseCase(startDate, endDate)
        }
    }
    
    private fun getStartOfWeek(): Long {
        val calendar = Calendar.getInstance()
        calendar.set(Calendar.DAY_OF_WEEK, Calendar.MONDAY)
        calendar.set(Calendar.HOUR_OF_DAY, 0)
        calendar.set(Calendar.MINUTE, 0)
        calendar.set(Calendar.SECOND, 0)
        calendar.set(Calendar.MILLISECOND, 0)
        return calendar.timeInMillis
    }
    
    private fun getTodayDate(): Long {
        val calendar = Calendar.getInstance()
        calendar.set(Calendar.HOUR_OF_DAY, 0)
        calendar.set(Calendar.MINUTE, 0)
        calendar.set(Calendar.SECOND, 0)
        calendar.set(Calendar.MILLISECOND, 0)
        return calendar.timeInMillis
    }
}
