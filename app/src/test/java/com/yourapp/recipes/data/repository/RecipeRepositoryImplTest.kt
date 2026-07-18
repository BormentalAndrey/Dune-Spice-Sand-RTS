package com.yourapp.recipes.data.repository

import com.google.gson.Gson
import com.yourapp.recipes.data.local.dao.IngredientDao
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import com.yourapp.recipes.domain.model.*
import io.mockk.*
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.flowOf
import kotlinx.coroutines.test.runTest
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class RecipeRepositoryImplTest {
    
    private lateinit var recipeDao: RecipeDao
    private lateinit var ingredientDao: IngredientDao
    private lateinit var repository: RecipeRepositoryImpl
    private val gson = Gson()
    
    @Before
    fun setUp() {
        recipeDao = mockk()
        ingredientDao = mockk()
        repository = RecipeRepositoryImpl(recipeDao, ingredientDao, gson)
    }
    
    @Test
    fun `getFilteredRecipes returns mapped domain models`() = runTest {
        // Given
        val entities = listOf(
            RecipeEntity(
                id = 1,
                title = "Борщ",
                ingredientsJson = """[{"id":"1","name":"Свекла","quantity":2,"unit":"шт","category":"vegetables"}]""",
                stepsJson = """[{"stepNumber":1,"description":"Варить"}]"""
            )
        )
        
        coEvery { recipeDao.getFilteredRecipes(any(), any(), any(), any(), any(), any()) } returns flowOf(entities)
        
        // When
        val result = repository.getFilteredRecipes(RecipeFilter()).first()
        
        // Then
        assertEquals(1, result.size)
        assertEquals("Борщ", result[0].title)
        assertEquals(1, result[0].ingredients.size)
        assertEquals("Свекла", result[0].ingredients[0].name)
    }
}
