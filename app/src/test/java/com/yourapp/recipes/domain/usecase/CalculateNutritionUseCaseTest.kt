package com.yourapp.recipes.domain.usecase

import com.yourapp.recipes.domain.model.NutritionInfo
import com.yourapp.recipes.domain.usecase.recipe.CalculateNutritionUseCase
import org.junit.Assert.assertEquals
import org.junit.Before
import org.junit.Test

class CalculateNutritionUseCaseTest {
    
    private lateinit var calculateNutritionUseCase: CalculateNutritionUseCase
    
    @Before
    fun setUp() {
        calculateNutritionUseCase = CalculateNutritionUseCase()
    }
    
    @Test
    fun `calculate nutrition for 2 servings returns doubled values`() {
        // Given
        val baseNutrition = NutritionInfo(
            calories = 250f,
            proteins = 15f,
            fats = 10f,
            carbohydrates = 30f
        )
        val multiplier = 2f
        
        // When
        val result = calculateNutritionUseCase(baseNutrition, multiplier)
        
        // Then
        assertEquals(500f, result.calories)
        assertEquals(30f, result.proteins)
        assertEquals(20f, result.fats)
        assertEquals(60f, result.carbohydrates)
    }
    
    @Test
    fun `calculate nutrition for half serving returns halved values`() {
        // Given
        val baseNutrition = NutritionInfo(
            calories = 400f,
            proteins = 20f,
            fats = 15f,
            carbohydrates = 45f
        )
        val multiplier = 0.5f
        
        // When
        val result = calculateNutritionUseCase(baseNutrition, multiplier)
        
        // Then
        assertEquals(200f, result.calories)
        assertEquals(10f, result.proteins)
        assertEquals(7.5f, result.fats)
        assertEquals(22.5f, result.carbohydrates)
    }
}
