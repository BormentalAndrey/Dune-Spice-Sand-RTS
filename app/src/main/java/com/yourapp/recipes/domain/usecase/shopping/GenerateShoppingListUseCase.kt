package com.yourapp.recipes.domain.usecase.shopping

import com.yourapp.recipes.data.local.dao.MealPlanDao
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.domain.repository.ShoppingListRepository
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.yourapp.recipes.domain.model.Ingredient
import kotlinx.coroutines.flow.first
import javax.inject.Inject

class GenerateShoppingListUseCase @Inject constructor(
    private val mealPlanDao: MealPlanDao,
    private val recipeDao: RecipeDao,
    private val shoppingListRepository: ShoppingListRepository,
    private val gson: Gson
) {
    suspend operator fun invoke(startDate: Long, endDate: Long) {
        val mealPlans = mealPlanDao.getMealPlansForPeriod(startDate, endDate).first()
        val ingredientsType = object : TypeToken<List<Ingredient>>() {}.type
        
        // Собираем все ингредиенты из плана питания
        val allIngredients = mealPlans.flatMap { mealPlan ->
            val ingredients: List<Ingredient> = gson.fromJson(
                mealPlan.recipe.ingredientsJson, 
                ingredientsType
            ) ?: emptyList()
            
            ingredients.map { ingredient ->
                ingredient.copy(
                    quantity = ingredient.quantity * mealPlan.mealPlan.servings
                )
            }
        }
        
        // Группируем и суммируем одинаковые ингредиенты
        val groupedIngredients = allIngredients.groupBy { 
            "${it.name}_${it.unit}" 
        }.map { (_, ingredients) ->
            ingredients.reduce { acc, ingredient ->
                acc.copy(quantity = acc.quantity + ingredient.quantity)
            }
        }
        
        // Очищаем текущий список и добавляем новые ингредиенты
        shoppingListRepository.resetAllItems()
        groupedIngredients.forEach { ingredient ->
            shoppingListRepository.addIngredientsToShoppingList(
                listOf(ingredient),
                servings = 1
            )
        }
    }
}
