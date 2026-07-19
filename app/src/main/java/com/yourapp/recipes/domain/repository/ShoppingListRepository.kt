package com.yourapp.recipes.domain.repository

import com.yourapp.recipes.data.local.dao.CategorySpending
import com.yourapp.recipes.domain.model.Ingredient
import com.yourapp.recipes.domain.model.ShoppingItem
import kotlinx.coroutines.flow.Flow

interface ShoppingListRepository {
    fun getAllItems(): Flow<List<ShoppingItem>>
    fun getItemsGroupedByCategory(): Flow<Map<String, List<ShoppingItem>>>
    fun getTotalRemainingPrice(): Flow<Float>
    fun getTotalSpentSince(startDate: Long): Flow<Float>
    fun getSpendingByCategory(startDate: Long): Flow<List<CategorySpending>>
    suspend fun addItem(item: ShoppingItem)
    suspend fun addIngredientsToShoppingList(
        ingredients: List<Ingredient>,
        servings: Int = 1,
        recipeId: Long? = null
    )
    suspend fun updatePurchasedStatus(itemId: Long, isPurchased: Boolean)
    suspend fun updateItemPrice(itemId: Long, price: Float)
    suspend fun deleteItem(itemId: Long)
    suspend fun clearPurchasedItems()
    suspend fun resetAllItems()
    suspend fun generateFromMealPlan(recipeIds: List<Long>, servings: Map<Long, Int>)
}
